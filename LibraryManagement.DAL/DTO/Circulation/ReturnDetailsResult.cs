namespace LibraryManagementDAL.DTO.Circulation
{
    public class ReturnDetailsResult
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public List<CirculationBorrowDetailItem> UnreturnedDetails { get; set; } = new();
    }
}
