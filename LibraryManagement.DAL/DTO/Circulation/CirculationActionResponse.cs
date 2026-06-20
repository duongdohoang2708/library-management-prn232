namespace LibraryManagementDAL.DTO.Circulation
{
    public class CirculationActionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? TransactionId { get; set; }
    }
}
