using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.DTO.Inventory;

public class InventoryListApiResult
{
    public List<BookCopy> Items { get; set; } = new();
    public int TotalPages { get; set; } = 1;
    public int CurrentPage { get; set; } = 1;
}

public class BookCopiesApiResult
{
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public List<BookCopy> Items { get; set; } = new();
}
