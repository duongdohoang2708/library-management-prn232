using LibraryManagementDAL.Models;

namespace LibraryManagement.BLL.DTO.Inventory
{
    public class InventoryListResult
    {
        public List<BookCopy> Items { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
