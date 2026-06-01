using System.ComponentModel.DataAnnotations;
namespace LibraryManagementDAL.Models
{
    // Thông tin đầu sách (Book Header) - Không phải cuốn sách vật lý
    public class Book : BaseEntity
    {
        public int BookId { get; set; }

        // Tựa đề sách
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        // Mã số sách quốc tế (ISBN)
        [MaxLength(20)]
        public string ISBN { get; set; } = null!;

        // Mô tả nội dung sách
        public string? Description { get; set; }

        // Ảnh bìa sách
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // Năm xuất bản lần đầu
        public int? PublishYear { get; set; }

        // ====== THÔNG N TIN TÁI BẢN ======

        // Lần tái bản thứ mấy (mặc định là 1)
        [Range(1, 100)]
        public int EditionNumber { get; set; } = 1;

        // Năm tái bản (có thể null nếu chưa tái bản)
        public int? ReprintYear { get; set; }

        // =================================

        // Khóa ngoại - Tác giả
        [Required]
        public int AuthorId { get; set; }
        public Author Author { get; set; }

        // Khóa ngoại - Thể loại
        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Khóa ngoại - Nhà xuất bản
        [Required]
        public int PublisherId { get; set; }
        public Publisher Publisher { get; set; }
        public bool IsActive { get; set; }

        // ================= QUAN HỆ NAVIGATIONS =================

        // Danh sách các bản sao vật lý của sách này (Các cuốn sách cụ thể trên kệ)
        public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();

        // Tóm tắt nội dung sách do AI tạo
        public BookAISummary? BookAISummary { get; set; }
        // Danh sách người dùng đặt trước sách này
        public ICollection<Reservation> Reservations { get; set; }

        // Các đánh giá về sách
        public ICollection<BookReview> BookReviews { get; set; }
    }
}
