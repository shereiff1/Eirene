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
                .ToListAsync();
        }

        public async Task<(List<CommunityGroup> Items, int TotalCount)> GetAllWithDetailsPagedAsync(int page, int pageSize)
        {
            var query = _context.Set<CommunityGroup>()
                .Include(g => g.CreatedBy)
                .Include(g => g.Posts)
                .Include(g => g.Members)
                .OrderByDescending(g => g.CreatedAt)
                .AsSplitQuery();
                
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<CommunityGroup?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.CreatedBy)
                .Include(g => g.Posts)
                .Include(g => g.Members)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<CommunityGroup?> GetByNameAsync(string name)
        {
            return await _context.Set<CommunityGroup>()
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
                .ToListAsync();
        }

        public async Task<CommunityGroup?> GetByIdWithMembersAsync(Guid id)
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<CommunityGroup>> GetJoinedGroupsByUserIdAsync(string userId)
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.Posts)
                .Where(g => g.UserCommunityGroups!.Any(ucg => ucg.UserId == userId))
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CommunityGroup>> GetUnjoinedGroupsByUserIdAsync(string userId)
        {
            return await _context.Set<CommunityGroup>()
                .Include(g => g.Posts)
                .Where(g => !g.UserCommunityGroups!.Any(ucg => ucg.UserId == userId))
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }
    }
}
