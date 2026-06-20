using LibraryManagementDAL.Models;

namespace LibraryManagementDAL.DTO.Reservation
{
    public class ReservationItem
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int? BookCopyId { get; set; }
        public string? Barcode { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime? ExpireAt { get; set; }
        public ReservationStatus Status { get; set; }
    }
}
