using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.Models.Community.Membership;

public class CommunityGroupUserTimeoutRequest : CommunityGroupUserActionRequest
{
    [Required]
    public DateTime TimeoutUntil { get; set; }
}
