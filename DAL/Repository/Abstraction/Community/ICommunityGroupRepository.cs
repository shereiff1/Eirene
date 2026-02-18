using DAL.Entities.Community;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstraction.Community
{
    public interface ICommunityGroupRepository : IGenericRepository<CommunityGroup>
    {
        Task<List<CommunityGroup>> GetAllWithDetailsAsync();
        Task<CommunityGroup?> GetByIdWithDetailsAsync(Guid id);
        Task<CommunityGroup?> GetByNameAsync(string name);
        Task<List<CommunityGroup>> GetByUserIdAsync(string userId);
    }
}
