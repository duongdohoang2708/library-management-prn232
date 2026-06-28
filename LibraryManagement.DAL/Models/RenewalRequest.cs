using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementDAL.Models
{
    public class RenewalRequest : BaseEntity
    {
        public int RenewalRequestId { get; set; }

        public int UserId { get; set; }
        public Account Account { get; set; } = null!;
        [NotMapped]
        public Account User { get => Account; set => Account = value; }

        public int BorrowDetailId { get; set; }
        public BorrowDetail BorrowDetail { get; set; } = null!;

        public int RequestedExtraDays { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public RenewalRequestStatus Status { get; set; } = RenewalRequestStatus.Pending;

        public int? ReviewedByUserId { get; set; }
        public Account? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime? NewDueDate { get; set; }
    }

    public enum RenewalRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3
    }
}
