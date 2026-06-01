// Log ghi lại các yêu cầu gọi tới AI
public class AIRequestLog
{
    public int AIRequestLogId { get; set; }

    // Loại đối tượng gọi AI (Ví dụ: Book)
    public string EntityType { get; set; }
    // ID của đối tượng (Ví dụ: BookId = 1)
    public int? EntityId { get; set; }

    // Tên model AI sử dụng (GPT-4, Gemini...)
    public string ModelName { get; set; }

    // Số tokens tiêu thụ (để tính chi phí)
    public int? TokensUsed { get; set; }

    // Thời gian xử lý tính bằng mili-giây
    public long? DurationMs { get; set; }

    // Trạng thái request (Success, Failed)
    public string Status { get; set; }

    // Thông báo lỗi nếu có
    public string? ErrorMessage { get; set; }

    // Thời gian thực hiện request
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Chi tiết nội dung Prompt và Response (Lưu bảng riêng để tối ưu DB)
    public AIRequestLogDetail Detail { get; set; }
}
