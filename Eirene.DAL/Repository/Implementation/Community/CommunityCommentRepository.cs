using Eirene.DAL.Entities.Community;
using Eirene.DAL.Repository.Abstraction.Community;
using Eirene.DAL.Database;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Community
{
    internal class CommunityCommentRepository : GenericRepository<CommunityComment>, ICommunityCommentRepository
    {
        public CommunityCommentRepository(EireneDBContext context) : base(context)
        {
        }
        public async Task<CommunityComment?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Set<CommunityComment>()
                .Include(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<CommunityComment>> GetByPostIdWithDetailsAsync(Guid postId)
        {
            return await _context.Set<CommunityComment>()
                .Include(c => c.User)
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.PostedOn)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<CommunityComment>> GetRepliesByCommentIdAsync(Guid commentId)
        {
            return await _context.Set<CommunityComment>()
                .Include(c => c.User)
                .Where(c => c.ParentCommentId == commentId)
                .OrderBy(c => c.PostedOn)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
