namespace FitwomanAPI.DTOs.Auth;

public class RegisterResponseDto
{
    public string Message { get; set; } = string.Empty;
    public UserProfileDto User { get; set; } = null!;
}
