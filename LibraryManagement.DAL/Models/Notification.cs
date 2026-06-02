using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public class Notification : BaseEntity
    {
        public int NotificationId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public string Type { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsSent { get; set; } = false;

        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }
    }

}