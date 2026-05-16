using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.Models.Community.Membership;

public class CommunityGroupUserActionRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
