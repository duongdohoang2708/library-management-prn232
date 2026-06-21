using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public class SystemSetting : BaseEntity
    {
        public int SystemSettingId { get; set; }

        [Required, MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Value { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }
    }
}
