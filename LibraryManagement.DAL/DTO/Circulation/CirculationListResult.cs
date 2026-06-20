namespace LibraryManagementDAL.DTO.Circulation
{
    public class CirculationListResult
    {
        public List<CirculationTransactionItem> Items { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}
