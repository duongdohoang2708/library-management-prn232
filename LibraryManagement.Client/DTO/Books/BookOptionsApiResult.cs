using LibraryManagementDAL.Models;

namespace LibraryManagement.Client.DTO.Books
{
    public class BookOptionsApiResult
    {
        public List<Author> Authors { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Publisher> Publishers { get; set; } = new();
    }
}
