using LibraryManagementDAL.Models;

namespace LibraryManagement.BLL.DTO.Books
{
    public class BookListResult
    {
        public List<Book> Items { get; set; } = new();
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public List<Category> Categories { get; set; } = new();
        public List<Publisher> Publishers { get; set; } = new();
        public List<int> PublishYears { get; set; } = new();
    }
}
