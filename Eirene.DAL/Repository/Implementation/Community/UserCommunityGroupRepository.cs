using Eirene.DAL.Database;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Repository.Abstraction.Community;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Community;

public class UserCommunityGroupRepository : GenericRepository<UserCommunityGroup>, IUserCommunityGroupRepository
{
    public UserCommunityGroupRepository(EireneDBContext context)
        : base(context)
    {
    }

    public async Task<UserCommunityGroup?> GetByGroupAndUserAsync(Guid groupId, string userId)
    {
        return await _context.UserCommunityGroups
            .FirstOrDefaultAsync(ug => ug.CommunityGroupId == groupId && ug.UserId == userId);
    }

    public async Task<UserCommunityGroup?> GetByGroupAndUserWithDetailsAsync(Guid groupId, string userId)
    {
        return await _context.UserCommunityGroups
            .Include(ug => ug.User)
            .Include(ug => ug.CommunityGroup)
            .FirstOrDefaultAsync(ug => ug.CommunityGroupId == groupId && ug.UserId == userId);
    }

    public async Task<List<UserCommunityGroup>> GetBannedUsersByGroupAsync(Guid groupId)
    {
        return await _context.UserCommunityGroups
            .Include(ug => ug.User)
            .Where(ug => ug.CommunityGroupId == groupId && ug.IsBanned)
            .ToListAsync();
    }

    public async Task<List<UserCommunityGroup>> GetTimedOutUsersByGroupAsync(Guid groupId)
    {
        var now = DateTime.UtcNow;
        return await _context.UserCommunityGroups
            .Include(ug => ug.User)
            .Where(ug => ug.CommunityGroupId == groupId && ug.TimeoutUntil.HasValue && ug.TimeoutUntil.Value > now)
            .ToListAsync();
    }
}
