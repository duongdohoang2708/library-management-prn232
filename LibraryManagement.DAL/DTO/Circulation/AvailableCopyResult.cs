namespace LibraryManagementDAL.DTO.Circulation
{
    public class AvailableCopyResult
    {
        public string Barcode { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}
