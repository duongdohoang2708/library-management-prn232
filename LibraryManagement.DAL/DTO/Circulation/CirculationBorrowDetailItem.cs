namespace LibraryManagementDAL.DTO.Circulation
{
    public class CirculationBorrowDetailItem
    {
        public int BorrowDetailId { get; set; }
        public int? BookCopyId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public decimal? FineAmount { get; set; }
    }
}
