namespace LibraryManagementDAL.DTO.Circulation
{
    public class UserSearchResult
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
