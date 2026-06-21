namespace LibraryManagement.BLL.DTO.Audit
{
    public class AuditLogItem
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
}
