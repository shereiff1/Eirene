namespace Eirene.BLL.Models.Community.Membership;

public class ApplyCommunityGroupEnforcementPolicy
{
    public bool IsBanned { get; set; }
    public string BanReason { get; set; } = string.Empty;
    public DateTime? MessagingTimeoutStartsAt { get; set; }
    public DateTime? MessagingTimeoutEndsAt { get; set; }
    public string MessagingTimeoutReason { get; set; } = string.Empty;
}
