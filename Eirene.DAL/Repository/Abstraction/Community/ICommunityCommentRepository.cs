using Eirene.DAL.Entities.Community;

namespace Eirene.DAL.Repository.Abstraction.Community
{
    public interface ICommunityCommentRepository : IGenericRepository<CommunityComment>
    {
        Task<CommunityComment?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<CommunityComment>> GetByPostIdWithDetailsAsync(Guid postId);
        Task<IEnumerable<CommunityComment>> GetRepliesByCommentIdAsync(Guid commentId);
    }
}
