namespace LibraryManagement.BLL.DTO.Auth
{
    public class VerifyResetCodeRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
