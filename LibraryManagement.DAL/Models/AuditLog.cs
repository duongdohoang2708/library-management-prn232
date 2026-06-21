using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public class AuditLog : BaseEntity
    {
        public int AuditLogId { get; set; }

        public int? ActorUserId { get; set; }

        [MaxLength(150)]
        public string ActorName { get; set; } = "System";

        [Required, MaxLength(80)]
        public string Action { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string EntityName { get; set; } = string.Empty;

        [MaxLength(80)]
        public string? EntityId { get; set; }

        [MaxLength(1000)]
        public string Summary { get; set; } = string.Empty;
    }
}
