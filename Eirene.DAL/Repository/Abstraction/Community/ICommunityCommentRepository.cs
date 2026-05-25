using Eirene.DAL.Entities.Community;

namespace Eirene.DAL.Repository.Abstraction.Community
{
    public interface ICommunityCommentRepository : IGenericRepository<CommunityComment>
    {
        Task<CommunityComment?> GetByIdWithDetailsAsync(Guid id);
        Task<List<CommunityComment>> GetByPostIdWithDetailsAsync(Guid postId);
        Task<List<CommunityComment>> GetRepliesByCommentIdAsync(Guid commentId);
    }
}
