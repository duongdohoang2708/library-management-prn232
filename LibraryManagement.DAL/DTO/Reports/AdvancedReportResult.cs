namespace LibraryManagementDAL.DTO.Reports
{
    public class AdvancedReportResult
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<MostBorrowedBookItem> MostBorrowedBooks { get; set; } = new();
        public List<OverdueUserItem> TopOverdueUsers { get; set; } = new();
        public List<FineRevenueMonthItem> FineRevenueByMonth { get; set; } = new();
    }

    public class MostBorrowedBookItem
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int BorrowCount { get; set; }
    }

    public class OverdueUserItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int OverdueCount { get; set; }
        public decimal TotalFine { get; set; }
    }

    public class FineRevenueMonthItem
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
