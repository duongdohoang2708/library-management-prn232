using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Thông tin người dùng hệ thống
namespace LibraryManagementDAL.Models
{
    [Table("Accounts")]
    public class Account : BaseEntity
    {
        [Key]
        public int UserId { get; set; }

        // Tên đăng nhập (bắt buộc, tối đa 50 ký tự)
        [Required, MaxLength(50)]
        public string Username { get; set; }

        // Email người dùng (bắt buộc, duy nhất)
        [Required, MaxLength(100)]
        public string Email { get; set; }

        // Mật khẩu đã mã hóa (Hash) bằng PasswordHasher - KHÔNG lưu plain text
        [Required]
        public string PasswordHash { get; set; }

        // Họ và tên đầy đủ
        [Required, MaxLength(150)]
        public string FullName { get; set; }

        // Số điện thoại
        [MaxLength(15)]
        public string? Phone { get; set; }

        // Địa chỉ thường trú
        [MaxLength(255)]
        public string? Address { get; set; }

        // Ngày sinh
        public DateTime? DateOfBirth { get; set; }

        // Trạng thái kích hoạt tài khoản (true = hoạt động, false = bị khóa)
        public bool IsActive { get; set; } = true;

        // Thời điểm đăng nhập lần cuối
        public DateTime? LastLoginAt { get; set; }

        // Google Subject ID (dùng khi đăng nhập bằng Google)
        [MaxLength(255)]
        public string? GoogleId { get; set; }

        // Đánh dấu người dùng đã thiết lập mật khẩu thực sự hay chưa (quan trọng cho Google OAuth)
        public bool IsPasswordSet { get; set; } = true;

        // =================== JWT REFRESH TOKEN ================
        // Refresh Token (dùng để cấp lại Access Token khi hết hạn)
        [MaxLength(512)]
        public string? RefreshToken { get; set; }

        // Thời điểm hết hạn của Refresh Token
        public DateTime? RefreshTokenExpiry { get; set; }

        [MaxLength(6)]
        public string? PasswordResetCode { get; set; }

        public DateTime? PasswordResetCodeExpiresAt { get; set; }

        public DateTime? PasswordResetCodeVerifiedAt { get; set; }

        // ================= QUAN HỆ NAVIGATIONS =================
        public Member? Member { get; set; }

        public Staff? Staff { get; set; }

        // Danh sách vai trò của người dùng (N-N)
        // Lịch sử mượn sách
        public ICollection<BorrowTransaction>? BorrowTransactions { get; set; }

        // Các thông báo nhận được
        public ICollection<Notification>? Notifications { get; set; }

        // Danh sách đặt chỗ trước
        public ICollection<Reservation>? Reservations { get; set; }

        // Lịch sử thanh toán
        public ICollection<Payment>? Payments { get; set; }

        // Các đánh giá sách đã viết
        public ICollection<BookReview>? BookReviews { get; set; }
    }

}
