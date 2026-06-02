using LibraryManagementDAL.Models;
using System.ComponentModel.DataAnnotations;

// Tóm tắt sách do AI tạo ra
public class BookAISummary : BaseEntity
{
    public int BookAISummaryId { get; set; }

    // Thuộc về sách nào
    public int BookId { get; set; }
    public Book Book { get; set; }

    // Nội dung tóm tắt
    [Required]
    public string SummaryText { get; set; }

    // Model nào đã tạo ra tóm tắt này (để so sánh chất lượng)
    public string ModelName { get; set; }

    // Số tokens đã dùng để tạo tóm tắt
    public int? TokensUsed { get; set; }
}
