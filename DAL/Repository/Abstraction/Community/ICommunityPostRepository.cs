using DAL.Entities.Community;

namespace DAL.Repository.Abstraction.Community
{
    public interface ICommunityPostRepository : IGenericRepository<CommunityPost>
    {
        Task<List<CommunityPost>> GetAllWithDetailsAsync();
        Task<List<CommunityPost>> GetByGroupIdWithDetailsAsync(Guid groupId);
        Task<CommunityPost?> GetByIdWithDetailsAsync(Guid id);
        Task<List<CommunityPost>> GetByUserIdWithDetailsAsync(string userId);
    }
}
