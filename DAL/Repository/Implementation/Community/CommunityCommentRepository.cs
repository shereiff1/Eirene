using DAL.Entities.Community;
using DAL.Repository.Abstraction.Community;
using DAL.Database;
using DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Community
{
    public class CommunityCommentRepository : GenericRepository<CommunityComment>, ICommunityCommentRepository
    {
        public CommunityCommentRepository(EireneDBContext context) : base(context)
        {
        }
        public async Task<CommunityComment?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<CommunityComment>()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id.Equals(id));
        }

        public async Task<IEnumerable<CommunityComment>> GetByPostIdWithDetailsAsync(int postId)
        {
            return await _context.Set<CommunityComment>()
                .Include(c => c.User)
                .Where(c => c.PostId.Equals(postId))
                .OrderByDescending(c => c.PostedOn)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommunityComment>> GetRepliesByCommentIdAsync(int commentId)
        {
            return await _context.Set<CommunityComment>()
                .Include(c => c.User)
                .Where(c => c.ParentCommentId.Equals(commentId))
                .OrderBy(c => c.PostedOn)
                .ToListAsync();
        }


    }
}
