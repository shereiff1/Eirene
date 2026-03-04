using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Identity;

public class ForgotPasswordDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
