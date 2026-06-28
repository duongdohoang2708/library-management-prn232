using LibraryManagementDAL.Models;

namespace LibraryManagementDAL.DTO.RenewalRequest
{
    public class RenewalRequestItem
    {
        public int RenewalRequestId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int BorrowDetailId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public DateTime CurrentDueDate { get; set; }
        public int RequestedExtraDays { get; set; }
        public DateTime RequestedAt { get; set; }
        public RenewalRequestStatus Status { get; set; }
        public DateTime? NewDueDate { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByName { get; set; }
    }
}
