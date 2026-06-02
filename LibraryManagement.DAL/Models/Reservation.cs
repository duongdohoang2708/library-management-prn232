using LibraryManagementDAL.Models;
using System.ComponentModel.DataAnnotations;

// Phiếu đặt trước sách (Reservation)
public class Reservation : BaseEntity
{
    public int ReservationId { get; set; }

    // Người đặt
    public int UserId { get; set; }

    // Đầu sách muốn đặt (Doraemon Tập 1)
    public int BookId { get; set; }

    // Cuốn sách cụ thể được giữ (sau khi hệ thống xử lý allocate)
    public int? BookCopyId { get; set; }
    public BookCopy? BookCopy { get; set; }

    // Ngày thực hiện đặt chỗ
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

    // Thời hạn giữ sách (Nếu đến hạn không lấy sẽ bị hủy)
    public DateTime? ExpireAt { get; set; }

    // Trạng thái phiếu đặt
    public ReservationStatus Status { get; set; }

    // Phiên bản Row để xử lý đồng thời
    [Timestamp]
    public byte[] RowVersion { get; set; }

    // ================= QUAN HỆ NAVIGATIONS =================
    public User User { get; set; }
    public Book Book { get; set; }
}

// Enum trạng thái đặt chỗ
public enum ReservationStatus
{
    Pending = 0,     // Chờ có sách (Sách đang hết, chờ người khác trả)
    Allocated = 1,   // Đã giữ sách cho user (Sách đã về và được giữ cho phiếu này)
    Completed = 2,   // Đã hoàn thành (User đã đến lấy sách -> chuyển sang Borrow)
    Cancelled = 3,   // Đã hủy (User tự hủy hoặc Admin hủy)
    Expired = 4      // Hết hạn giữ sách (User không đến lấy đúng hạn)
}
