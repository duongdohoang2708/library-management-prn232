using System.ComponentModel.DataAnnotations;

// Chi tiết nội dung log AI (Tách ra bảng riêng vì text rất dài)
public class AIRequestLogDetail
{
    [Key]
    public int AIRequestLogDetailId { get; set; }

    // Liên kết 1-1 với AIRequestLog
    public int AIRequestLogId { get; set; }
    public AIRequestLog AIRequestLog { get; set; }

    // Nội dung gửi đi (Prompt)
    public string Prompt { get; set; }

    // Nội dung nhận về (Response)
    public string Response { get; set; }
}
