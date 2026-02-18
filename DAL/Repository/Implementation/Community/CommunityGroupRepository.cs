using DAL.Entities.Community;
using DAL.Repository.Abstraction.Community;
using DAL.Database;
using DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Community
{
    public class CommunityGroupRepository : GenericRepository<CommunityGroup>, ICommunityGroupRepository
    {
        public CommunityGroupRepository(EireneDBContext context)
            : base(context)
        {
        }

        public async Task<List<CommunityGroup>> GetAllWithDetailsAsync()
        {
            return await _context.CommunityGroups
                .Include(g => g.CreatedBy)
                .Include(g => g.Posts)
                .Include(g => g.Members)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<CommunityGroup?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.CommunityGroups
                .Include(g => g.CreatedBy)
                .Include(g => g.Posts)
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id.Equals(id));
        }

        public async Task<CommunityGroup?> GetByNameAsync(string name)
        {
            return await _context.CommunityGroups
                .FirstOrDefaultAsync(g => g.Name == name);
        }

        public async Task<List<CommunityGroup>> GetByUserIdAsync(string userId)
        {
            return await _context.CommunityGroups
                .Include(g => g.CreatedBy)
                .Include(g => g.Posts)
                .Where(g => g.CreatedByUserId == userId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }
    }
}
