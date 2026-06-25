using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.DAL.Enumerators;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IDoctorProfileService
    {
        Task<Result<List<DoctorModel>>> GetAllAsync();
        Task<Result<DoctorModel>> GetByIdAsync(string id);
        Task<Result<DoctorModel>> CreateDoctorProfileAsync(AddDoctorProfile model, string userId);
        Task<Result<DoctorModel>> UpdateDoctorProfileAsync(EditDoctorProfile model, string userId);
        Task<Result> DeleteDoctorProfile(string doctorId);
        Task<Result<VerificationStatus>> CheckVerificationStatus(string doctorId);
    }
}
