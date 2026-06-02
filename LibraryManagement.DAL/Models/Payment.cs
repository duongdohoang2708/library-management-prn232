using System.ComponentModel.DataAnnotations;

// Giao dịch thanh toán tiền phạt hoặc phí
namespace LibraryManagementDAL.Models
{
    public class Payment : BaseEntity
    {
        public int PaymentId { get; set; }

        // Người thanh toán
        public int UserId { get; set; }

        // Liên kết giao dịch mượn (Optional - có thể thanh toán lẻ)
        public int? BorrowTransactionId { get; set; }

        // Tổng số tiền thanh toán
        public decimal Amount { get; set; }

        // Phương thức thanh toán (Tiền mặt, Chuyển khoản...)
        public PaymentMethod PaymentMethod { get; set; }

        // Trạng thái thanh toán
        public PaymentStatus PaymentStatus { get; set; }

        // Mã giao dịch ngân hàng (nếu có)
        public string? TransactionCode { get; set; }

        // Ngày giờ thanh toán thành công
        public DateTime? PaidAt { get; set; }

        // Phiên bản Row để xử lý đồng thời
        [Timestamp]
        public byte[] RowVersion { get; set; }

        // ================= QUAN HỆ NAVIGATIONS =================
        public User User { get; set; }
        public BorrowTransaction? BorrowTransaction { get; set; }

        // Chi tiết các khoản được thanh toán
        public ICollection<PaymentDetail> PaymentDetails { get; set; }
            = new List<PaymentDetail>();
    }

    // Enum phương thức thanh toán
    public enum PaymentMethod
    {
        Cash = 0,    // Tiền mặt
        VnPay = 3    // Cổng VnPay
    }

    // Enum trạng thái thanh toán
    public enum PaymentStatus
    {
        Pending = 0,  // Đang chờ xử lý
        Success = 1,  // Thành công
        Failed = 2,   // Thất bại
        Refunded = 3  // Đã hoàn tiền
    }

}