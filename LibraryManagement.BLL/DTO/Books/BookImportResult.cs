namespace LibraryManagement.BLL.DTO.Books
{
    public class BookImportResult
    {
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
