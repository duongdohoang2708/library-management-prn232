using LibraryManagementDAL.Models;

namespace LibraryManagement.BLL.DTO.Inventory
{
    public class UpdateCopyRequest
    {
        public int BookId { get; set; }
        public BookCopyStatus Status { get; set; }
        public BookCondition Condition { get; set; }
        public string? Location { get; set; }
    }
}
