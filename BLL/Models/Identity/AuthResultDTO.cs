namespace BLL.Models.Identity;

public class AuthResultDTO
{
    public bool Success { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string[] Errors { get; set; } = Array.Empty<string>();
    public string? EmailVerificationCode { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime? EmailVerificationExpiry { get; set; }
    public bool EmailConfirmed { get; set; } = false;
}
