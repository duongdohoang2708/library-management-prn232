using LibraryManagementDAL.Models;

public class BookReview : BaseEntity
{
    public int BookReviewId { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }

    public int Rating { get; set; }   // 1 -> 5
    public string? Comment { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public Book Book { get; set; }
}
