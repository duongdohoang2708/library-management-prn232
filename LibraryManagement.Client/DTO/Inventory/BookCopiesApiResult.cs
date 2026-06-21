using LibraryManagementDAL.Models;

namespace LibraryManagement.Client.DTO.Inventory
{
    public class BookCopiesApiResult
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public List<BookCopy> Items { get; set; } = new();
    }
}
