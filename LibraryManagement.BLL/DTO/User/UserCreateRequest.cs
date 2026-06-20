using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.BLL.DTO.User
{
    public class UserCreateRequest
    {
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string? Role { get; set; }
        public string? Password { get; set; }
        public string? PasswordHash { get; set; }
        public string? ComfirmPasswordHash { get; set; }
    }
}
