using LibraryManagementDAL.Models;

namespace LibraryManagement.Client.DTO.Books
{
    public class BookListApiResult
    {
        public List<Book> Items { get; set; } = new();
        public int TotalPages { get; set; } = 1;
        public int Page { get; set; } = 1;
        public List<Category> Categories { get; set; } = new();
        public List<Publisher> Publishers { get; set; } = new();
        public List<int> PublishYears { get; set; } = new();
    }
}
