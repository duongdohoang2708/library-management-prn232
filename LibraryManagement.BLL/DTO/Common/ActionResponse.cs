namespace LibraryManagement.BLL.DTO.Common
{
    public class ActionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Id { get; set; }
    }
}
