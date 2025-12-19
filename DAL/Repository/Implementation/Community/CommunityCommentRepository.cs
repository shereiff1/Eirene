using DAL.Entities.Community;
using DAL.Repository.Abstraction.Community;
using DAL.Database;
using DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Community
{
    public class CommunityCommentRepository : GenericRepository<CommunityComment>, ICommunityCommentRepository
    {
        public CommunityCommentRepository(EireneDBContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
        public async Task<CommunityComment?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<CommunityComment>()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<CommunityComment>> GetByPostIdWithDetailsAsync(int postId)
        {
            return await _context.Set<CommunityComment>()
                .Include(c => c.User)
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.PostedOn)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommunityComment>> GetRepliesByCommentIdAsync(int commentId)
        {
            return await _context.Set<CommunityComment>()
                .Include(c => c.User)
                .Where(c => c.ParentCommentId == commentId)
                .OrderBy(c => c.PostedOn)
                .ToListAsync();
        }


    }
}
