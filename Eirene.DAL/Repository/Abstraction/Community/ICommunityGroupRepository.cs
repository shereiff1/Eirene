using Eirene.DAL.Entities.Community;

namespace Eirene.DAL.Repository.Abstraction.Community
{
    public interface ICommunityGroupRepository : IGenericRepository<CommunityGroup>
    {
        Task<List<CommunityGroup>> GetAllWithDetailsAsync();
        Task<CommunityGroup?> GetByIdWithDetailsAsync(Guid id);
        Task<CommunityGroup?> GetByNameAsync(string name);
        Task<List<CommunityGroup>> GetByUserIdAsync(string userId);
        Task<CommunityGroup?> GetByIdWithMembersAsync(Guid id);
        Task<List<CommunityGroup>> GetJoinedGroupsByUserIdAsync(string userId);
        Task<List<CommunityGroup>> GetUnjoinedGroupsByUserIdAsync(string userId);
    }
}
