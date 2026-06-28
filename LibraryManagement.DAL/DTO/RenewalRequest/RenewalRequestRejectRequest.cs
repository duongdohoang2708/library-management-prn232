namespace LibraryManagementDAL.DTO.RenewalRequest
{
    public class RenewalRequestRejectRequest
    {
        public int ReviewerUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
