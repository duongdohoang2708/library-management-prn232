namespace LibraryManagementDAL.DTO.Circulation
{
    public class CirculationTransactionItem
    {
        public int BorrowTransactionId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<CirculationBorrowDetailItem> BorrowDetails { get; set; } = new();
    }
}
