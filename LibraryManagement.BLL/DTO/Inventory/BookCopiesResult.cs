using LibraryManagementDAL.Models;

namespace LibraryManagement.BLL.DTO.Inventory
{
    public class BookCopiesResult
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public List<BookCopy> Items { get; set; } = new();
    }
}
