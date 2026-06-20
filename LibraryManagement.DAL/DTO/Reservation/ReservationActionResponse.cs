namespace LibraryManagementDAL.DTO.Reservation
{
    public class ReservationActionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ReservationId { get; set; }
        public int? TransactionId { get; set; }
    }
}
