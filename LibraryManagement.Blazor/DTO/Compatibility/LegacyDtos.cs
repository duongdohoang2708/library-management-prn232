using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.DTO.Book
{
    public class BookCreateModelRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PublishYear { get; set; }
        public int EditionNumber { get; set; }
        public int? AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public int? PublisherId { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
    }

    public class BookUpdateModelRequest : BookCreateModelRequest
    {
        public int BookId { get; set; }
    }

    public class ImportBooksResult
    {
        public List<string> Errors { get; set; } = new();
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public int TotalRows { get; set; }
    }
}

namespace LibraryManagementDAL.DTO.User
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserCreateRequest : UserProfileDto
    {
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string ComfirmPasswordHash { get; set; } = string.Empty;
    }
}

namespace LibraryManagementDAL.DTO.Notification
{
    public class NotificationCreateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }
}

namespace LibraryManagementDAL.DTO.Pagination
{
    public class PaginationResponseModel<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
