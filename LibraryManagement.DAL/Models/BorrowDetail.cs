using System.ComponentModel.DataAnnotations;

// Chi tiết mượn sách - Quản lý từng cuốn sách trong một lần mượn
namespace LibraryManagementDAL.Models
{
    public class BorrowDetail : BaseEntity
    {
        public int BorrowDetailId { get; set; }

        // Thuộc về giao dịch mượn nào
        public int BorrowTransactionId { get; set; }
        public BorrowTransaction BorrowTransaction { get; set; }

        // Mượn cuốn sách cụ thể nào
        public int? BookCopyId { get; set; }
        public BookCopy? BookCopy { get; set; }

        // Ngày bắt đầu mượn cuốn này
        public DateTime BorrowDate { get; set; }

        // Hạn trả cho cuốn này
        public DateTime DueDate { get; set; }

        // Ngày thực tế trả sách (nếu null là chưa trả)
        public DateTime? ActualReturnDate { get; set; }

        // Số tiền phạt phát sinh (quá hạn, hỏng...)
        public decimal? FineAmount { get; set; }

        // Số tiền phạt đã được thanh toán
        public decimal? FinePaidAmount { get; set; }

        // Cờ đánh dấu đã trả hết phạt chưa
        public bool IsFinePaid { get; set; }

        // Phiên bản Row để xử lý đồng thời (Optimistic Concurrency)
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

}