using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    // Bản sao vật lý của một cuốn sách (Cuốn sách thực tế trên kệ)
    public class BookCopy : BaseEntity
{
    public int BookCopyId { get; set; }

    // Thuộc về đầu sách nào
    public int BookId { get; set; }
    public Book Book { get; set; }

    // Mã vạch dán trên sách (Duy nhất để quét mượn trả)
    [Required]
    [MaxLength(50)]
    public string Barcode { get; set; }

    // Trạng thái hiện tại của cuốn sách (Có sẵn, Đang mượn, Mất...)
    [Required]
    public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;

    // Tình trạng vật lý của sách (Mới, Cũ, Hư hỏng...)
    [Required]
    public BookCondition Condition { get; set; } = BookCondition.Good;

    // Vị trí lưu trữ trong thư viện (Kệ A, Ngăn 2...)
    [MaxLength(100)]
    public string? Location { get; set; }

    // Phiên bản Row để xử lý đồng thời (Optimistic Concurrency) - Ngăn chặn 2 người cùng sửa
    [Timestamp]
    public byte[] RowVersion { get; set; }

    // ================= QUAN HỆ NAVIGATIONS =================

    // Lịch sử mượn trả liên quan đến cuốn sách này
    public ICollection<BorrowDetail> BorrowDetails { get; set; }
        = new List<BorrowDetail>();

    // Danh sách đặt chỗ liên quan đến cuốn này
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

// Enum trạng thái sách
public enum BookCopyStatus
{
    Available = 0,   // Có thể mượn
    Borrowed = 1,    // Đang được mượn
    Reserved = 2,    // Đã được đặt giữ cho ai đó
    Lost = 3,        // Bị mất
    Damaged = 4      // Bị hư hỏng không thể dùng
}

// Enum tình trạng vật lý
public enum BookCondition
{
    New = 0,         // Sách mới
    Good = 1,        // Sách còn tốt
    Fair = 2,        // Sách hơi cũ nhưng dùng được
    Poor = 3,        // Sách cũ nát
    Damaged = 4      // Sách đã hỏng
}
}
