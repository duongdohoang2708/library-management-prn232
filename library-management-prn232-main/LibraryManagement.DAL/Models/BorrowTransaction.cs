using System.ComponentModel.DataAnnotations;

// Yêu cầu mượn sách (Transaction) - Quản lý chung một lần mượn
namespace LibraryManagementDAL.Models
{
    public class BorrowTransaction : BaseEntity
    {
        public int BorrowTransactionId { get; set; }

        // Người mượn
        public int UserId { get; set; }
        public User User { get; set; }

        // Ngày tạo phiếu mượn
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        // Hạn trả chung cho lần mượn này (Có thể khác hạn trả từng cuốn nếu gia hạn lẻ)
        [Required]
        public DateTime DueDate { get; set; }

        // Trạng thái giao dịch: Borrowing, Returned, Overdue...
        [Required]
        public string Status { get; set; } = "Borrowing";

        // ================= QUAN HỆ NAVIGATIONS =================

        // Chi tiết các cuốn sách được mượn trong giao dịch này
        public ICollection<BorrowDetail> BorrowDetails { get; set; }

        // Lịch sử thanh toán phạt/phí liên quan
        public ICollection<Payment> Payments { get; set; }
    }
}
