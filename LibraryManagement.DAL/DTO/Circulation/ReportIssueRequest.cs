using System.ComponentModel.DataAnnotations;
using LibraryManagementDAL.Models;

namespace LibraryManagementDAL.DTO.Circulation
{
    public class ReportIssueRequest
    {
        [Range(1, int.MaxValue)]
        public int BorrowDetailId { get; set; }

        public BookCopyStatus Status { get; set; }

        [Range(0, double.MaxValue)]
        public decimal FineAmount { get; set; }
    }
}
