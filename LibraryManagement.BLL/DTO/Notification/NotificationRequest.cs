using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.BLL.DTO.Notification
{
    public class NotificationRequest
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "General";
    }
}
