

using System.Text.Json.Serialization;

namespace Eirene.BLL.Models.Identity;

public class RegistrationDTO
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    [JsonIgnore]
    public bool Success { get; set; }
}
