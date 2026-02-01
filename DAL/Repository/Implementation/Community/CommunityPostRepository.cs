using DAL.Database;
using DAL.Entities.Community;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Community;
using DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Community
{
    public class CommunityPostRepository : GenericRepository<CommunityPost>, ICommunityPostRepository
    {
        public CommunityPostRepository(EireneDBContext context)
            : base(context)
        {
        }
        private IQueryable<CommunityPost> IncludePostDetails()
        {
            return _context.CommunityPosts
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

        public async Task<List<CommunityPost>> GetByGroupIdWithDetailsAsync(int groupId)
        {
            return await IncludePostDetails()
                .Where(p => p.CommunityGroupId == groupId)
                .OrderByDescending(p => p.PostedOn)
                .ToListAsync();
        }

        public async Task<CommunityPost?> GetByIdWithDetailsAsync(int id)
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
