using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.BLL.DTO.Reviews
{
    public class ReviewSubmitRequest
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int BookId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}
