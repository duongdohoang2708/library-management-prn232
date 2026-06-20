using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.DTO.Circulation
{
    public class RenewRequest
    {
        [Range(1, int.MaxValue)]
        public int BorrowDetailId { get; set; }

        [Range(1, 30)]
        public int ExtraDays { get; set; } = 7;
    }
}
