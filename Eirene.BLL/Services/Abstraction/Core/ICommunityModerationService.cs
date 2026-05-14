using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Community.Membership;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface ICommunityModerationService
    {
        Task<Result> ManageCommunityGroupMembershipAsync(Guid groupId, string userId, bool assign);
        Task<Result> BanUserFromGroupAsync(Guid groupId, string userId);
        Task<Result> UnbanUserFromGroupAsync(Guid groupId, string userId);
        Task<Result> TimeoutUserInGroupAsync(Guid groupId, string userId, DateTime timeoutUntil);
        Task<Result> RemoveTimeoutUserInGroupAsync(Guid groupId, string userId);
        Task<Result<List<CommunityGroupMembershipDTO>>> GetBannedUsersByGroupAsync(Guid groupId);
        Task<Result<List<CommunityGroupMembershipDTO>>> GetTimedOutUsersByGroupAsync(Guid groupId);
    }
}
