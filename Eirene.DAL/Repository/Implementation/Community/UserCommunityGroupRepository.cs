using Eirene.DAL.Database;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Repository.Abstraction.Community;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Community;

internal class UserCommunityGroupRepository : GenericRepository<UserCommunityGroup>, IUserCommunityGroupRepository
{
    public UserCommunityGroupRepository(EireneDBContext context)
        : base(context)
    {
    }

    public async Task<UserCommunityGroup?> GetByGroupAndUserAsync(Guid groupId, string userId)
    {
        return await _context.Set<UserCommunityGroup>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ug => ug.CommunityGroupId == groupId && ug.UserId == userId);
    }

    public async Task<UserCommunityGroup?> GetByGroupAndUserWithDetailsAsync(Guid groupId, string userId)
    {
        return await _context.Set<UserCommunityGroup>()
            .Include(ug => ug.User)
            .Include(ug => ug.CommunityGroup)
            .FirstOrDefaultAsync(ug => ug.CommunityGroupId == groupId && ug.UserId == userId);
    }

    public async Task<List<UserCommunityGroup>> GetBannedUsersByGroupAsync(Guid groupId)
    {
        return await _context.Set<UserCommunityGroup>()
            .Include(ug => ug.User)
            .Where(ug => ug.CommunityGroupId == groupId && ug.IsBanned)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<UserCommunityGroup>> GetTimedOutUsersByGroupAsync(Guid groupId)
    {
        var now = DateTime.UtcNow;
        return await _context.Set<UserCommunityGroup>()
            .Include(ug => ug.User)
            .Where(ug => ug.CommunityGroupId == groupId && ug.TimeoutUntil.HasValue && ug.TimeoutUntil.Value > now)
            .AsNoTracking()
            .ToListAsync();
    }
}
