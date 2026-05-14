using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IRoleManagementService
    {
        Task<Result> AssignRoleAsync(string adminId, string userId, string role);
        Task<Result<List<DoctorModel>>> GetPendingDoctorsAsync();
        Task<Result> ApproveDoctorAsync(string doctorId);
    }
}
