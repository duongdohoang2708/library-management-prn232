using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.BLL.DTO.User
{
    public class UserProfileUpdateRequest
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
