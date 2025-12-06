using DAL.Entities.Community;

namespace DAL.Repository.Abstraction.Community
{
    public interface ICommunityCommentRepository : IGenericRepository<CommunityComment>
    {
        Task<CommunityComment?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<CommunityComment>> GetByPostIdWithDetailsAsync(int postId);
        Task<IEnumerable<CommunityComment>> GetRepliesByCommentIdAsync(int commentId);
    }
}
