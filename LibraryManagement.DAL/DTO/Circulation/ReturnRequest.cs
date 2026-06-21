using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.DTO.Circulation
{
    public class ReturnRequest
    {
        [Required]
        public List<int> BorrowDetailIds { get; set; } = new();

        [MaxLength(500)]
        public string? ConditionRemarks { get; set; }
    }
}
