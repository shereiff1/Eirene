using BLL.Models.Community.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Abstraction.Community
{
    public interface ICommunityGroupServices
    {
        Task<(bool IsSuccess, List<CommunityGroupDTO>? Groups)> GetAllAsync();
        Task<(bool IsSuccess, CommunityGroupDTO? Group)> GetByIdAsync(Guid id);
        Task<(bool IsSuccess, List<CommunityGroupDTO>? Groups)> GetByUserIdAsync(string userId);
        Task<(bool IsSuccess, CommunityGroupDTO? CreatedGroup)> CreateAsync(AddCommunityGroup model);
        Task<bool> UpdateAsync(EditCommunityGroup model);
        Task<(bool IsSuccess, CommunityGroupWithDetails? Group)> GetByIdWithFullDetailsAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
    }
}
