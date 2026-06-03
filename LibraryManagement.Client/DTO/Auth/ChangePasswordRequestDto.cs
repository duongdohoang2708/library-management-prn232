using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Client.DTO.Auth
{
    public class ChangePasswordRequestDto
    {

        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare(nameof(NewPassword), ErrorMessage = "Confirm password does not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
