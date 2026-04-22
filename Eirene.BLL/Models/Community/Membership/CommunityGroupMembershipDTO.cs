namespace Eirene.BLL.Models.Community.Membership;

public class CommunityGroupMembershipDTO
{
    public Guid Id { get; set; }
    public Guid CommunityGroupId { get; set; }
    public string CommunityGroupName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsEnrolled { get; set; }
    public bool IsBanned { get; set; }
    public string BanReason { get; set; } = string.Empty;
    public DateTime? BannedAt { get; set; }
    public string BannedByUserId { get; set; } = string.Empty;
    public DateTime? MessagingTimeoutStartsAt { get; set; }
    public DateTime? MessagingTimeoutEndsAt { get; set; }
    public string MessagingTimeoutReason { get; set; } = string.Empty;
    public string TimeoutSetByUserId { get; set; } = string.Empty;
    public bool HasActiveMessagingTimeout { get; set; }
    public bool CanSendMessages { get; set; }
}
