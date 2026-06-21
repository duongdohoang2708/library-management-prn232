using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public class Role : BaseEntity
    {
        public int RoleId { get; set; }

        [Required, MaxLength(50)]
        public string RoleName { get; set; }

        public ICollection<Staff> Staffs { get; set; } = new List<Staff>();
    }
}
