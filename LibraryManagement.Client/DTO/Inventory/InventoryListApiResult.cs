using LibraryManagementDAL.Models;

namespace LibraryManagement.Client.DTO.Inventory
{
    public class InventoryListApiResult
    {
        public List<BookCopy> Items { get; set; } = new();
        public int TotalPages { get; set; } = 1;
        public int CurrentPage { get; set; } = 1;
    }
}
