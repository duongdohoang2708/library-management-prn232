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
        private readonly CirculationRepository _circulationRepository;
        private readonly ReservationRepository _reservationRepository;
        private readonly UserManagementRepository _userManagementRepository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AIService(
            BookRepository bookRepository,
            CirculationRepository circulationRepository,
            ReservationRepository reservationRepository,
            UserManagementRepository userManagementRepository,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _bookRepository = bookRepository;
            _circulationRepository = circulationRepository;
            _reservationRepository = reservationRepository;
            _userManagementRepository = userManagementRepository;
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
                await LogAiRequestAsync("Book", bookId, prompt, responseText, durationMs, "gemini-2.5-flash-lite");
                return null;
            }

            var summary = new BookAISummary
            {
                BookId = bookId,
                SummaryText = responseText,
                ModelName = "gemini-2.5-flash-lite",
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

            // Load user data context if user is logged in
            string userContext = "";
            if (currentUserId.HasValue)
            {
                var account = await _userManagementRepository.GetAccountAsync(currentUserId.Value);
                if (account != null)
                {
                    var sbUser = new StringBuilder();
                    sbUser.AppendLine("--- THÔNG TIN NGƯỜI DÙNG ĐĂNG NHẬP (BẠN ĐANG TRÒ CHUYỆN VỚI NGƯỜI NÀY) ---");
                    sbUser.AppendLine($"Mã người dùng (User ID): {account.UserId}");
                    sbUser.AppendLine($"Họ tên: {account.FullName}");
                    sbUser.AppendLine($"Email: {account.Email}");
                    sbUser.AppendLine($"Username: {account.Username}");
                    if (!string.IsNullOrEmpty(account.Phone)) sbUser.AppendLine($"Số điện thoại: {account.Phone}");
                    if (!string.IsNullOrEmpty(account.Address)) sbUser.AppendLine($"Địa chỉ: {account.Address}");

                    // 1. Active Borrows
                    var transactions = await _circulationRepository.QueryTransactions()
                        .Where(x => x.UserId == account.UserId)
                        .ToListAsync();

                    var activeBorrows = transactions
                        .SelectMany(t => t.BorrowDetails)
                        .Where(d => d.ActualReturnDate == null)
                        .ToList();

                    sbUser.AppendLine($"Số sách hiện tại đang mượn (chưa trả): {activeBorrows.Count} cuốn.");
                    if (activeBorrows.Any())
                    {
                        sbUser.AppendLine("Chi tiết các sách đang mượn:");
                        int idx = 1;
                        foreach (var detail in activeBorrows)
                        {
                            var bookTitle = detail.BookCopy?.Book?.Title ?? "Không rõ";
                            var author = detail.BookCopy?.Book?.Author?.Name ?? "Không rõ";
                            var barcode = detail.BookCopy?.Barcode ?? "Không rõ";
                            var remainingDays = (detail.DueDate.Date - DateTime.UtcNow.Date).Days;
                            var statusStr = remainingDays >= 0 ? $"Còn {remainingDays} ngày để trả" : $"Đã quá hạn {-remainingDays} ngày";
                            sbUser.AppendLine($"  {idx++}. Tựa đề: '{bookTitle}' | Tác giả: {author} | Mã vạch: {barcode} | Ngày mượn: {detail.BorrowDate:dd/MM/yyyy} | Hạn trả: {detail.DueDate:dd/MM/yyyy} ({statusStr})");
                        }
                    }

                    // 2. Return History
                    var returnedBorrows = transactions
                        .SelectMany(t => t.BorrowDetails)
                        .Where(d => d.ActualReturnDate != null)
                        .OrderByDescending(d => d.ActualReturnDate)
                        .ToList();

                    sbUser.AppendLine($"Số sách đã mượn và đã trả trong quá khứ: {returnedBorrows.Count} cuốn.");
                    if (returnedBorrows.Any())
                    {
                        sbUser.AppendLine("Lịch sử sách đã trả (tối đa 5 cuốn gần nhất):");
                        int idx = 1;
                        foreach (var detail in returnedBorrows.Take(5))
                        {
                            var bookTitle = detail.BookCopy?.Book?.Title ?? "Không rõ";
                            var author = detail.BookCopy?.Book?.Author?.Name ?? "Không rõ";
                            sbUser.AppendLine($"  {idx++}. Tựa đề: '{bookTitle}' | Tác giả: {author} | Ngày mượn: {detail.BorrowDate:dd/MM/yyyy} | Ngày trả: {detail.ActualReturnDate:dd/MM/yyyy}");
                        }
                    }

                    // 3. Reservations
                    var reservations = await _reservationRepository.QueryUserReservations(account.UserId).ToListAsync();
                    var activeReservations = reservations
                        .Where(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Allocated)
                        .ToList();

                    sbUser.AppendLine($"Số sách đang đặt trước (chờ mượn): {activeReservations.Count} cuốn.");
                    if (activeReservations.Any())
                    {
                        sbUser.AppendLine("Chi tiết các sách đang đặt trước:");
                        int idx = 1;
                        foreach (var res in activeReservations)
                        {
                            var bookTitle = res.Book?.Title ?? "Không rõ";
                            var author = res.Book?.Author?.Name ?? "Không rõ";
                            var statusStr = res.Status == ReservationStatus.Allocated ? "Đã có sách giữ cho bạn (hãy đến thư viện nhận)" : "Đang chờ hàng đợi (đang chờ người khác trả)";
                            sbUser.AppendLine($"  {idx++}. Tựa đề: '{bookTitle}' | Tác giả: {author} | Trạng thái đặt: {statusStr} | Ngày đặt: {res.ReservedAt:dd/MM/yyyy}");
                        }
                    }

                    // 4. Unpaid Fines
                    var unpaidDetails = transactions
                        .SelectMany(t => t.BorrowDetails)
                        .Where(d => !d.IsFinePaid && d.FineAmount.GetValueOrDefault() > d.FinePaidAmount.GetValueOrDefault())
                        .ToList();

                    decimal totalUnpaidFine = unpaidDetails.Sum(d => d.FineAmount.GetValueOrDefault() - d.FinePaidAmount.GetValueOrDefault());
                    sbUser.AppendLine($"Tổng số tiền phạt chưa thanh toán: {totalUnpaidFine:N0} VNĐ.");
                    if (unpaidDetails.Any())
                    {
                        sbUser.AppendLine("Chi tiết các sách có tiền phạt chưa thanh toán:");
                        int idx = 1;
                        foreach (var detail in unpaidDetails)
                        {
                            var bookTitle = detail.BookCopy?.Book?.Title ?? "Không rõ";
                            var unpaidAmount = detail.FineAmount.GetValueOrDefault() - detail.FinePaidAmount.GetValueOrDefault();
                            sbUser.AppendLine($"  {idx++}. Tựa đề: '{bookTitle}' | Số tiền phạt còn lại: {unpaidAmount:N0} VNĐ | Lý do phạt: {(detail.ActualReturnDate == null ? "Chưa trả sách quá hạn" : "Trả sách quá hạn hoặc bị hỏng/mất")}");
                        }
                    }

                    sbUser.AppendLine("------------------------------------------------------------------");
                    userContext = sbUser.ToString();
                }
            }

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
            var prompt = $"Bạn là một thủ thư AI thân thiện, am hiểu và nhiệt tình của thư viện LMS (Library Management System). Hãy trả lời câu hỏi sau của người dùng bằng tiếng Việt một cách lịch sự, ngắn gọn và hữu ích: {userMessage}\n\n" +
                          "Lưu ý quan trọng khi trả lời về việc mượn sách:\n" +
                          "- Phân biệt rõ giữa 'ĐANG mượn' (sách chưa trả, số lượng chưa trả) và 'ĐÃ mượn' (sách đã trả trong quá khứ/lịch sử).\n" +
                          "- Nếu người dùng hỏi 'đang mượn những cuốn nào/bao nhiêu cuốn' hoặc tương tự, và dữ liệu cho thấy họ đang mượn 0 cuốn nhưng đã từng mượn sách trong lịch sử, hãy trả lời rõ ràng: 'Hiện tại bạn không có cuốn sách nào đang mượn (chúng đều đã được trả). Tuy nhiên, trong lịch sử bạn đã từng mượn cuốn [Tên sách] và đã hoàn thành việc trả vào ngày [Ngày trả]'.\n" +
                          "- Tránh trả lời phủ định tuyệt đối như 'bạn chưa từng mượn cuốn nào' hay 'bạn chưa mượn sách' nếu thực tế lịch sử của họ có ghi nhận giao dịch đã trả.";
            
            if (!string.IsNullOrEmpty(userContext))
            {
                prompt += $"\n\nĐây là thông tin của người dùng đang chat với bạn (HÃY DÙNG dữ liệu này nếu người dùng hỏi về bản thân họ, sách họ mượn/trả/đặt trước, tiền phạt, thông tin tài khoản, v.v.):\n{userContext}";
            }

            if (!string.IsNullOrEmpty(libraryContext))
            {
                prompt += $"\n\nĐây là dữ liệu thời gian thực từ thư viện (bạn HÃY DÙNG dữ liệu này để trả lời câu hỏi của người dùng nếu liên quan):\n{libraryContext}";
            }

            var responseText = await CallGeminiApiAsync(prompt);
            var durationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            await LogAiRequestAsync("Chat", currentUserId, prompt, responseText, durationMs, "gemini-2.5-flash-lite");

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

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={_apiKey}";
            
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
