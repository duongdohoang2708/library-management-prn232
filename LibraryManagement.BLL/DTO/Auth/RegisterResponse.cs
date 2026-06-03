namespace LibraryManagement.BLL.DTO.Auth
{
    public class RegisterResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }
}
