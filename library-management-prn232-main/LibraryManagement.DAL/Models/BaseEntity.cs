using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public abstract class BaseEntity
    {
        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}