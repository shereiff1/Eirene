using Eirene.DAL.Entities.Community;
using Eirene.DAL.Repository.Abstraction.Community;
using Eirene.DAL.Database;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Community
{
    internal class CommunityGroupRepository : GenericRepository<CommunityGroup>, ICommunityGroupRepository
    {
        public CommunityGroupRepository(EireneDBContext context)
            : base(context)
        {
        }

        public async Task<List<CommunityGroup>> GetAllWithDetailsAsync()
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.CreatedBy)
                .Include(g => g.Posts)
                .Include(g => g.Members)
                .OrderByDescending(g => g.CreatedAt)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CommunityGroup?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.CreatedBy)
                .Include(g => g.Posts)
                .Include(g => g.Members)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<CommunityGroup?> GetByNameAsync(string name)
        {
            return await _context.Set<CommunityGroup>()
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Name == name);
        }

        public async Task<List<CommunityGroup>> GetByUserIdAsync(string userId)
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.CreatedBy)
                .Include(g => g.Posts)
                .Where(g => g.CreatedByUserId == userId)
                .OrderByDescending(g => g.CreatedAt)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CommunityGroup?> GetByIdWithMembersAsync(Guid id)
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.Members)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<CommunityGroup>> GetJoinedGroupsByUserIdAsync(string userId)
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.Posts)
                .Where(g => g.UserCommunityGroups!.Any(ucg => ucg.UserId == userId))
                .OrderByDescending(g => g.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<CommunityGroup>> GetUnjoinedGroupsByUserIdAsync(string userId)
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.Posts)
                .Where(g => !g.UserCommunityGroups!.Any(ucg => ucg.UserId == userId))
                .OrderByDescending(g => g.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
