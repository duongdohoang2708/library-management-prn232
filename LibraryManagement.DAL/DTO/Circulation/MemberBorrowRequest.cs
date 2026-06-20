using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.DTO.Circulation
{
    public class MemberBorrowRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please login before borrowing.")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a book.")]
        public int BookId { get; set; }

        [Range(1, 90)]
        public int LoanDays { get; set; } = 14;
    }
}
