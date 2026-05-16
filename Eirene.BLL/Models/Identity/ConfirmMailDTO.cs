using System.Text.Json.Serialization;

namespace Eirene.BLL.Models.Identity;

public class ConfirmMailDTO
{
    public string Message { get; set; } = string.Empty;
    [JsonIgnore]
    public bool Success { get; set; }
    [JsonIgnore]
    public string ErrorCode { get; set; } = string.Empty;
}