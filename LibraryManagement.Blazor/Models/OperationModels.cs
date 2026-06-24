using Microsoft.AspNetCore.Components.Forms;
using LibraryManagement.Blazor.DTO.Books;
using LibraryManagementDAL.DTO.Dashboard;
using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.Models;

public class OperationResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static OperationResult Success(string message) => new() { IsSuccess = true, Message = message };

    public static OperationResult Failure(string message, IEnumerable<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors?.ToArray() ?? Array.Empty<string>()
    };
}

public sealed class OperationResult<T> : OperationResult
{
    public T? Value { get; init; }

    public static OperationResult<T> Success(T? value, string message) => new()
    {
        IsSuccess = true,
        Message = message,
        Value = value
    };

    public new static OperationResult<T> Failure(string message, IEnumerable<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors?.ToArray() ?? Array.Empty<string>()
    };
}

public sealed class FollowUpAction
{
    public string Text { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public sealed class DownloadedFileResult
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
    public byte[] Content { get; init; } = Array.Empty<byte>();
}

public sealed class HomePageViewModel
{
    public List<Book> Books { get; init; } = new();
    public List<Book> PopularBooks { get; init; } = new();
    public List<Category> Categories { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

public sealed class AdminDashboardViewModel
{
    public DashboardStatsResult Stats { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

public sealed class DashboardPageViewModel
{
    public DashboardStatsResult Stats { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

public sealed class BookListQuery
{
    public string? Keyword { get; set; }
    public string? Category { get; set; }
    public int? PublisherId { get; set; }
    public int? PublishYear { get; set; }
    public bool? IsActive { get; set; }
    public string? Availability { get; set; }
    public int? MinRating { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class BookDetailsViewModel
{
    public Book Book { get; init; } = new();
    public string? AiSummary { get; init; }
    public string? AiSummaryError { get; init; }
    public FollowUpAction? Action { get; init; }
}

public sealed class ReviewPageViewModel
{
    public Book Book { get; init; } = new();
    public List<BookReview> Reviews { get; init; } = new();
    public BookReview? UserReview { get; init; }
}

public sealed class BookMutationRequest
{
    public required LibraryManagementDAL.DTO.Book.BookCreateModelRequest Model { get; init; }
    public IBrowserFile? ImageFile { get; init; }
}

public sealed class BookUpdateMutationRequest
{
    public required LibraryManagementDAL.DTO.Book.BookUpdateModelRequest Model { get; init; }
    public IBrowserFile? ImageFile { get; init; }
}
