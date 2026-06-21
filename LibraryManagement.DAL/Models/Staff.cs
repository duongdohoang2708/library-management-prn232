using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public class Staff : BaseEntity
    {
        public int StaffId { get; set; }

        public int UserId { get; set; }
        public Account Account { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        [Required, MaxLength(30)]
        public string StaffCode { get; set; } = string.Empty;

        public DateTime HiredAt { get; set; }
    }
}
