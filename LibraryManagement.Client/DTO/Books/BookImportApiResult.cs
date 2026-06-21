namespace LibraryManagement.Client.DTO.Books
{
    public class BookImportApiResult
    {
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
