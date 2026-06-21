using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.DTO.Circulation
{
    public class BorrowRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a reader.")]
        public int UserId { get; set; }

        [Required]
        public List<string> Barcodes { get; set; } = new();

        [Range(1, 90)]
        public int LoanDays { get; set; } = 14;
    }
}
