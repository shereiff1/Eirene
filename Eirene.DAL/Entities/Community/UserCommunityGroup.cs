using Eirene.DAL.Entities.Core;

namespace Eirene.DAL.Entities.Community;

public class UserCommunityGroup
{
    public Guid CommunityGroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsBanned { get; set; }
    public DateTime? TimeoutUntil { get; set; }

    public CommunityGroup? CommunityGroup { get; set; }
    public ApplicationUser? User { get; set; }

    public bool HasActiveTimeout(DateTime utcNow)
    {
        return TimeoutUntil.HasValue && TimeoutUntil.Value > utcNow;
    }
}
