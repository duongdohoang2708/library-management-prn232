namespace LibraryManagement.Client.DTO.Admin
{
    public class LibraryPolicySettingsDto
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

    public class AuditLogItemDto
    {
        public int AuditLogId { get; set; }
        public int? ActorUserId { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ActionResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Id { get; set; }
    }

    public class ReminderRunResultDto
    {
        public int SentCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
