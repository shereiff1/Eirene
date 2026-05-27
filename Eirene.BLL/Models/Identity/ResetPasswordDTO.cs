using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.Models.Identity;

public class ResetPasswordDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}
