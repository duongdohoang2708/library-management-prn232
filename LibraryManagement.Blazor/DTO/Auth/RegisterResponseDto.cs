namespace LibraryManagement.Blazor.DTO.Auth;

public class RegisterResponseDto
{
    public bool IsSuccess { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? UserId { get; set; }
}
