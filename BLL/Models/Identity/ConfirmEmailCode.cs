namespace BLL.Models.Identity;

public class ConfirmEmailCode
{
    public bool IsConfirmed { get; set; }
    public string Email { get; set; } = string.Empty;
}