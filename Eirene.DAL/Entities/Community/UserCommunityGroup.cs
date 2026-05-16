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

    public void Ban()
    {
        if (IsBanned)
            throw new InvalidOperationException("User is already banned from this community group.");
        
        IsBanned = true;
        TimeoutUntil = null;
    }

    public void Unban()
    {
        if (!IsBanned)
            throw new InvalidOperationException("User is not banned from this community group.");
        
        IsBanned = false;
    }

    public void Timeout(DateTime timeoutUntil)
    {
        if (timeoutUntil <= DateTime.UtcNow)
            throw new ArgumentException("Timeout end date must be in the future.");
        
        TimeoutUntil = timeoutUntil.ToUniversalTime();
    }

    public void RemoveTimeout()
    {
        if (!TimeoutUntil.HasValue)
            throw new InvalidOperationException("User does not have an active timeout in this community group.");
        
        TimeoutUntil = null;
    }
}
