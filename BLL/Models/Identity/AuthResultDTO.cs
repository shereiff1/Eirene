using System.Text.Json.Serialization;

namespace BLL.Models.Identity;

public class AuthResultDTO
{
    public bool Success { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiration { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;

}
