namespace LibraryManagementDAL.DTO.RenewalRequest
{
    public class RenewalRequestCreateRequest
    {
        public int UserId { get; set; }
        public int BorrowDetailId { get; set; }
        public int ExtraDays { get; set; }
    }
}
