namespace LibraryManagement.BLL.DTO.Settings
{
    public class LibraryPolicySettings
    {
        public int DefaultLoanDays { get; set; } = 14;
        public int RenewDays { get; set; } = 7;
        public int ReservationHoldDays { get; set; } = 3;
        public int MaxOpenBorrowedBooks { get; set; } = 5;
        public int DueSoonReminderDays { get; set; } = 1;
        public decimal OverdueFinePerDay { get; set; } = 5000m;
        public decimal DamagedFine { get; set; } = 50000m;
        public decimal LostFine { get; set; } = 100000m;
    }
}
