using Eirene.DAL.Database;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Repository.Abstraction.Community;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Community
{
    internal class CommunityPostRepository : GenericRepository<CommunityPost>, ICommunityPostRepository
    {
        public CommunityPostRepository(EireneDBContext context)
            : base(context)
        {
        }
        private IQueryable<CommunityPost> IncludePostDetails()
        {
            return _context.Set<CommunityPost>()
                .Include(p => p.User)
                .Include(p => p.CommunityGroup)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.User);
        }


        public async Task<List<CommunityPost>> GetAllWithDetailsAsync()
        {
            return await IncludePostDetails()
                .OrderByDescending(p => p.PostedOn)
                .ToListAsync();
        }

        public async Task<List<CommunityPost>> GetByGroupIdWithDetailsAsync(Guid groupId)
        {
            return await IncludePostDetails()
                .Where(p => p.CommunityGroupId == groupId)
                .OrderByDescending(p => p.PostedOn)
                .ToListAsync();
        }

        public async Task<CommunityPost?> GetByIdWithDetailsAsync(Guid id)
        {
            return await IncludePostDetails()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<CommunityPost>> GetByUserIdWithDetailsAsync(string userId)
        {
            return await IncludePostDetails()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PostedOn)
                .ToListAsync();
        }
    }
}
