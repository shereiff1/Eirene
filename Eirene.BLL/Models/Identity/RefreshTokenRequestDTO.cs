using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.Models.Identity;
public class RefreshTokenRequestDTO
{
    [Required] public string AccessToken { get; set; } = string.Empty;
    [Required] public string RefreshToken { get; set; } = string.Empty;
}