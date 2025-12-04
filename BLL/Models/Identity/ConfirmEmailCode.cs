namespace BLL.Models.Identity;

public class ConfirmEmailCode
{
    public string Code { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}