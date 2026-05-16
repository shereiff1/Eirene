namespace Eirene.BLL.Models.Identity;

public class MessageResultDTO
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}
