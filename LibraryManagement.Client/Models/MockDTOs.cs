using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Client.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

namespace LibraryManagementBLL.Interfaces
{
    public interface IMockInterface
    {
    }
}

namespace LibraryManagementDAL.DTO.Book
{
    public class BookCreateModelRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
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

namespace LibraryManagementDAL.DTO.Auth
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class RegisterRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must be 6 digits")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare(nameof(NewPassword), ErrorMessage = "Confirm password does not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
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

public class UserFinesDTO
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalFine { get; set; }
    public decimal FinePaid { get; set; }
    public decimal FineBalance { get; set; }
    public decimal TotalUnpaidFine { get; set; }
    public int UnpaidBookCount { get; set; }
}
