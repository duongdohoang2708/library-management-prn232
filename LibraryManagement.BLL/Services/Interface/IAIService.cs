using LibraryManagementDAL.Models;
using System.Threading.Tasks;

namespace LibraryManagement.BLL.Services.Interface
{
    public interface IAIService
    {
        Task<BookAISummary?> GenerateBookSummaryAsync(int bookId, int? currentUserId);
        Task<BookAISummary?> GetBookSummaryAsync(int bookId);
        Task<string> ChatWithLibrarianAIAsync(string userMessage, int? currentUserId);
    }
}
