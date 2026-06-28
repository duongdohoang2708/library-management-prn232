namespace LibraryManagementDAL.DTO.RenewalRequest
{
    public class RenewalRequestActionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? RenewalRequestId { get; set; }
    }
}
