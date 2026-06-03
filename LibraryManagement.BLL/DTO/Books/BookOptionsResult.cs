using LibraryManagementDAL.Models;

namespace LibraryManagement.BLL.DTO.Books
{
    public class BookOptionsResult
    {
        public List<Author> Authors { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Publisher> Publishers { get; set; } = new();
    }
}
