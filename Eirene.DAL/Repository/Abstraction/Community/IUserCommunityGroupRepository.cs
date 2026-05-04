using Eirene.DAL.Entities.Community;

namespace Eirene.DAL.Repository.Abstraction.Community;

public interface IUserCommunityGroupRepository : IGenericRepository<UserCommunityGroup>
{
    Task<UserCommunityGroup?> GetByGroupAndUserAsync(Guid groupId, string userId);
    Task<UserCommunityGroup?> GetByGroupAndUserWithDetailsAsync(Guid groupId, string userId);
    Task<List<UserCommunityGroup>> GetBannedUsersByGroupAsync(Guid groupId);
    Task<List<UserCommunityGroup>> GetTimedOutUsersByGroupAsync(Guid groupId);
}
