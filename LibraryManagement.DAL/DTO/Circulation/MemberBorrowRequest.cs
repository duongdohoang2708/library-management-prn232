using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.DTO.Circulation
{
    public class MemberBorrowRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please login before borrowing.")]
        public int UserId { get; set; }

        /// <summary>List of BookIds to borrow (up to 5). Each BookId will be matched to an available copy.</summary>
        public List<int> BookIds { get; set; } = new();

        [Range(1, 90)]
        public int LoanDays { get; set; } = 14;
    }
}

