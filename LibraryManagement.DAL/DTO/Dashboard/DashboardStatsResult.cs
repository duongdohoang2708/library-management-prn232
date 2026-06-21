namespace LibraryManagementDAL.DTO.Dashboard
{
    public class DashboardStatsResult
    {
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalActiveBorrows { get; set; }
        public int OverdueBooksCount { get; set; }
        public decimal TotalUnpaidFines { get; set; }

        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlyBorrows { get; set; } = new();
        public List<int> MonthlyReturns { get; set; } = new();
        public List<int> MonthlyRegistrations { get; set; } = new();

        public List<DashboardNameCount> CategoryStats { get; set; } = new();
        public List<DashboardNameCount> BookStatusStats { get; set; } = new();
        public List<DashboardUserItem> RecentUsers { get; set; } = new();
        public List<DashboardPaymentItem> RecentPayments { get; set; } = new();
        public List<DashboardTransactionItem> RecentTransactions { get; set; } = new();
        public List<DashboardDebtorItem> TopDebtors { get; set; } = new();
    }

    public class DashboardNameCount
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DashboardUserItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardPaymentItem
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class DashboardTransactionItem
    {
        public int BorrowTransactionId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DashboardDebtorItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalUnpaidFine { get; set; }
        public int UnpaidBookCount { get; set; }
    }
}
