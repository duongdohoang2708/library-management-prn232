using LibraryManagementDAL.Models;

namespace LibraryManagementDAL.DTO.Catalog
{
    public class CatalogIndexResult
    {
        public List<Author> Authors { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Publisher> Publishers { get; set; } = new();
    }

    public class AuthorSaveRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
    }

    public class CategorySaveRequest
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class PublisherSaveRequest
    {
        public string PublisherName { get; set; } = string.Empty;
        public string? Address { get; set; }
    }

    public class CatalogActionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Id { get; set; }
    }
}
