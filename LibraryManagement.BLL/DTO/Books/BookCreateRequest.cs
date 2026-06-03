namespace LibraryManagement.BLL.DTO.Books
{
    public class BookCreateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PublishYear { get; set; }
        public int EditionNumber { get; set; }
        public int? AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public int? PublisherId { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
    }
}
