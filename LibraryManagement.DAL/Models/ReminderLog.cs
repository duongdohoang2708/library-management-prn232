using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public class ReminderLog : BaseEntity
    {
        public int ReminderLogId { get; set; }

        public int BorrowDetailId { get; set; }
        public BorrowDetail BorrowDetail { get; set; } = null!;

        [Required, MaxLength(40)]
        public string ReminderType { get; set; } = string.Empty;

        public DateTime ReminderDate { get; set; }
    }
}
