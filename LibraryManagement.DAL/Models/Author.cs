using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    public class Author : BaseEntity
    {
        public int AuthorId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }

        public string? Biography { get; set; }

        public ICollection<Book> Books { get; set; }
    }

}