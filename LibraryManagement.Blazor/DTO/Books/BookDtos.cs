using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.DTO.Books;

public class BookListApiResult
{
    public List<Book> Items { get; set; } = new();
    public int TotalPages { get; set; } = 1;
    public int Page { get; set; } = 1;
    public List<Category> Categories { get; set; } = new();
    public List<Publisher> Publishers { get; set; } = new();
    public List<int> PublishYears { get; set; } = new();
}

public class BookOptionsApiResult
{
    public List<Author> Authors { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Publisher> Publishers { get; set; } = new();
}

public class BookImportApiResult
{
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class BookSaveResult
{
    public int BookId { get; set; }
}
