using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.Models.Identity
{
    public class GoogleLoginDTO
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;

    }
}
