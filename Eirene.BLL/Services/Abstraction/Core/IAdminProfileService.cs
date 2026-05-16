using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Admin;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IAdminProfileService
    {
        Task<Result<List<AdminModel>>> GetAllAsync();
        Task<Result<AdminModel>> GetByIdAsync(string adminId);
        Task<Result<AdminModel>> CreateAdminProfileAsync(string userId);
    }
}
