using LibraryManagementDAL.Models;
using System.ComponentModel.DataAnnotations;

public class Publisher : BaseEntity
{
    public int PublisherId { get; set; }

    [Required, MaxLength(150)]
    public string PublisherName { get; set; }

    [MaxLength(255)]
    public string Address { get; set; }

    public ICollection<Book> Books { get; set; }
}
