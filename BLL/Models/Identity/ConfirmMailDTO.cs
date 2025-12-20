namespace BLL.Models.Identity;

public class ConfirmMailDTO
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}