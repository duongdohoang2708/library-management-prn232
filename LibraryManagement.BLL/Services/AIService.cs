using LibraryManagement.BLL.Services.Interface;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.BLL.Services
{
    public class AIService : IAIService
    {
        private readonly BookRepository _bookRepository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AIService(BookRepository bookRepository, IConfiguration configuration, HttpClient httpClient)
        {
            _bookRepository = bookRepository;
            _configuration = configuration;
            _httpClient = httpClient;
            _apiKey = _configuration["Gemini:ApiKey"] ?? string.Empty;
        }

        public async Task<BookAISummary?> GetBookSummaryAsync(int bookId)
        {
            var summary = await _bookRepository.GetBookSummaryAsync(bookId);
            // Self-healing: Nếu bản ghi trong Database là câu thông báo lỗi (do sự cố API lưu nhầm lúc trước),
            // bỏ qua nó để hệ thống tự động sinh lại bản tóm tắt mới.
            if (summary != null && summary.SummaryText != null && summary.SummaryText.StartsWith("Lỗi"))
            {
                return null;
            }
            return summary;
        }

        public async Task<BookAISummary?> GenerateBookSummaryAsync(int bookId, int? currentUserId)
        {
            var book = await _bookRepository.GetBookByIdAsync(bookId);
            if (book == null) return null;

            var prompt = $"Bạn là một AI tóm tắt sách chuyên nghiệp. Hãy tóm tắt nội dung cuốn sách có tựa đề '{book.Title}' một cách ngắn gọn, súc tích (khoảng 150-200 chữ), dễ hiểu và thu hút người đọc.";
            if (!string.IsNullOrEmpty(book.Description))
            {
                prompt += $" Đây là một phần mô tả về sách để bạn tham khảo: {book.Description}";
            }

            var startTime = DateTime.UtcNow;
            var responseText = await CallGeminiApiAsync(prompt);
            var durationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            if (responseText.StartsWith("Lỗi"))
            {
                // Log lỗi lại để Admin theo dõi, nhưng TUYỆT ĐỐI KHÔNG LƯU vào bảng BookAISummary
                await LogAiRequestAsync("Book", bookId, prompt, responseText, durationMs, "gemini-2.5-flash");
                return null;
            }

            var summary = new BookAISummary
            {
                BookId = bookId,
                SummaryText = responseText,
                ModelName = "gemini-2.5-flash",
                TokensUsed = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _bookRepository.SaveBookSummaryAsync(summary);
            await LogAiRequestAsync("Book", bookId, prompt, responseText, durationMs, "gemini-2.5-flash");

            return summary;
        }

        public async Task<string> ChatWithLibrarianAIAsync(string userMessage, int? currentUserId)
        {
            var startTime = DateTime.UtcNow;

            // 1. Phân tích ý định (Extract Keyword)
            string keyword = await ExtractSearchKeywordAsync(userMessage);

            // 2. Tìm kiếm trong Database (RAG Retrieval)
            string libraryContext = "";
            if (!string.IsNullOrEmpty(keyword) && keyword != "NO_SEARCH")
            {
                if (keyword == "ALL_CATEGORIES")
                {
                    var categories = await _bookRepository.GetCategoriesAsync();
                    libraryContext = "Danh sách các thể loại sách hiện có trong thư viện: " + string.Join(", ", categories.Select(c => c.CategoryName)) + ".";
                }
                else if (keyword == "ALL_AUTHORS")
                {
                    var authors = await _bookRepository.GetAuthorsAsync();
                    libraryContext = "Danh sách các tác giả hiện có sách trong thư viện: " + string.Join(", ", authors.Take(50).Select(a => a.Name)) + (authors.Count > 50 ? "..." : "") + ".";
                }
                else
                {
                    var query = _bookRepository.QueryBooks().Include(b => b.BookCopies);
                    
                    List<Book> searchResults;
                    if (keyword == "ALL" || keyword == "ALL_BOOKS")
                    {
                        searchResults = await query.OrderByDescending(b => b.CreatedAt).Take(100).ToListAsync();
                    }
                    else
                    {
                        var normalized = keyword.ToLower();
                        searchResults = await query.Where(b => b.Title.ToLower().Contains(normalized) || b.Author.Name.ToLower().Contains(normalized) || b.Category.CategoryName.ToLower().Contains(normalized)).Take(20).ToListAsync();
                    }

                    if (searchResults.Any())
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Dưới đây là thông tin chi tiết về các sách trong thư viện (hãy dùng dữ liệu này để trả lời nếu người dùng hỏi về bất kỳ sách nào, thống kê, hoặc tìm kiếm):");
                        int count = 1;
                        foreach (var book in searchResults)
                        {
                            int available = book.BookCopies?.Count(c => c.Status == BookCopyStatus.Available) ?? 0;
                            sb.AppendLine($"{count++}. Tựa đề: '{book.Title}' | Tác giả: {book.Author?.Name} | Thể loại: {book.Category?.CategoryName} | Năm XB: {book.PublishYear} | Số lượng còn: {available} cuốn.");
                        }
                        libraryContext = sb.ToString();
                    }
                    else
                    {
                        // Fallback: If specific search fails, maybe the AI extracted a bad keyword. Let's just load all books!
                        var fallbackResults = await query.OrderByDescending(b => b.CreatedAt).Take(50).ToListAsync();
                        var sb = new StringBuilder();
                        sb.AppendLine($"Hệ thống không tìm thấy kết quả khớp chính xác với '{keyword}', nhưng đây là danh sách các sách hiện có trong thư viện để bạn tham khảo trả lời:");
                        int count = 1;
                        foreach (var book in fallbackResults)
                        {
                            int available = book.BookCopies?.Count(c => c.Status == BookCopyStatus.Available) ?? 0;
                            sb.AppendLine($"{count++}. '{book.Title}' - Tác giả: {book.Author?.Name} - Thể loại: {book.Category?.CategoryName} (Còn {available} cuốn).");
                        }
                        libraryContext = sb.ToString();
                    }
                }
            }

            // 3. Gọi AI với Prompt đã bổ sung ngữ cảnh
            var prompt = $"Bạn là một thủ thư AI thân thiện, am hiểu và nhiệt tình của thư viện LMS (Library Management System). Hãy trả lời câu hỏi sau của người dùng bằng tiếng Việt một cách lịch sự, ngắn gọn và hữu ích: {userMessage}";
            
            if (!string.IsNullOrEmpty(libraryContext))
            {
                prompt += $"\n\nĐây là dữ liệu thời gian thực từ thư viện (bạn HÃY DÙNG dữ liệu này để trả lời câu hỏi của người dùng nếu liên quan):\n{libraryContext}";
            }

            var responseText = await CallGeminiApiAsync(prompt);
            var durationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            await LogAiRequestAsync("Chat", currentUserId, prompt, responseText, durationMs, "gemini-2.5-flash");

            return responseText;
        }

        private async Task<string> ExtractSearchKeywordAsync(string userMessage)
        {
            var prompt = $"Bạn là một bộ phân tích ngôn ngữ. Hãy phân tích câu nói của người dùng và trích xuất ra một 'Từ khóa' (Tiêu đề sách hoặc Tên tác giả) để tìm kiếm trong cơ sở dữ liệu.\nVí dụ:\n- 'Bạn có sách Rừng Na Uy không?' -> 'Rừng Na Uy'\n- 'Có sách nào của Haruki Murakami không?' -> 'Haruki Murakami'\n- 'Chào bạn' -> 'NO_SEARCH'\n- 'Thư viện có sách gì mới?' -> 'ALL'\n- 'Thư viện có những thể loại (category) nào?' -> 'ALL_CATEGORIES'\n- 'Thư viện có những tác giả nào?' -> 'ALL_AUTHORS'\nCâu hỏi của người dùng: '{userMessage}'\nChỉ trả về đúng MỘT từ khóa, không thêm bất kỳ văn bản, dấu ngoặc kép hay giải thích nào khác.";
            
            var response = await CallGeminiApiAsync(prompt);
            if (string.IsNullOrEmpty(response) || response.StartsWith("Lỗi")) return "NO_SEARCH";

            return response.Trim().Replace("'", "").Replace("\"", "");
        }

        private async Task<string> CallGeminiApiAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "Hệ thống chưa được cấu hình API Key cho AI (Gemini). Vui lòng liên hệ quản trị viên.";
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var document = JsonDocument.Parse(responseString);
                    var root = document.RootElement;
                    var candidates = root.GetProperty("candidates");
                    if (candidates.GetArrayLength() > 0)
                    {
                        var content = candidates[0].GetProperty("content");
                        var parts = content.GetProperty("parts");
                        if (parts.GetArrayLength() > 0)
                        {
                            return parts[0].GetProperty("text").GetString() ?? "";
                        }
                    }
                }
                return $"Lỗi khi gọi AI API: {response.StatusCode} - {responseString}";
            }
            catch (Exception ex)
            {
                return $"Lỗi hệ thống khi gọi AI: {ex.Message}";
            }
        }

        private async Task LogAiRequestAsync(string entityType, int? entityId, string prompt, string response, long durationMs, string modelName)
        {
            var log = new AIRequestLog
            {
                EntityType = entityType,
                EntityId = entityId,
                ModelName = modelName,
                Status = response.StartsWith("Lỗi") ? "Failed" : "Success",
                ErrorMessage = response.StartsWith("Lỗi") ? response : null,
                DurationMs = durationMs,
                CreatedAt = DateTime.UtcNow,
                Detail = new AIRequestLogDetail
                {
                    Prompt = prompt,
                    Response = response
                }
            };
            await _bookRepository.LogAIRequestAsync(log);
        }
    }
}
