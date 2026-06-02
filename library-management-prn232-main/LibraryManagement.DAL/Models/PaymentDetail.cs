using System.ComponentModel.DataAnnotations.Schema;

// Chi tiết thanh toán - Thanh toán tiền cho khoản nào
namespace LibraryManagementDAL.Models
{
    public class PaymentDetail : BaseEntity
    {
        // Kế thừa BaseEntity cũng được để có CreatedAt
        // Nhưng vì nó là bảng phụ chi tiết, có thể không cần BaseEntity nếu muốn đơn giản
        // Ở đây ta giữ cấu trúc đơn giản như đã tạo trước đó, hoặc thêm BaseEntity nếu cần.
        // Theo step 56 thì chưa có BaseEntity. Mình sẽ thêm comment vào properties.

        public int PaymentDetailId { get; set; }

        // Thuộc về giao dịch thanh toán nào
        public int PaymentId { get; set; }
        public Payment Payment { get; set; }

        // Thanh toán cho khoản phạt của cuốn sách nào
        public int BorrowDetailId { get; set; }
        public BorrowDetail BorrowDetail { get; set; }

        // Số tiền phân bổ cho detail này
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
    }

}