using DAL.Entities.Core;

namespace DAL.Repository.Abstraction.Core
{
    public interface IApplicationUserRepository : IGenericRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser?> GetUserWithProfilesAsync(string userId);
        Task<List<ApplicationUser>> GetUsersByRoleAsync(string roleName);
    }
}