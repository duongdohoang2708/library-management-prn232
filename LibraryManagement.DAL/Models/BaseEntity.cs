using System.ComponentModel.DataAnnotations;

namespace LibraryManagementDAL.Models
{
    // Lớp cơ sở cho các thực thể, chứa các trường thông tin chung
    public abstract class BaseEntity
    {
        // Ngày tạo bản ghi (mặc định là thời gian hiện tại theo UTC)
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Ngày cập nhật bản ghi gần nhất (có thể null nếu chưa cập nhật lần nào)
        public DateTime? UpdatedAt { get; set; }

        // Cờ đánh dấu xóa mềm (Soft Delete): false = chưa xóa, true = đã xóa
        public bool IsDeleted { get; set; } = false;

        // Ngày thực hiện xóa mềm
        public DateTime? DeletedAt { get; set; }
    }
}
