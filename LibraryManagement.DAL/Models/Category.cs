using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public class Category : BaseEntity
    {
        public int CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
