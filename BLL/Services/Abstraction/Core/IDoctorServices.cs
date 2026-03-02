
using BLL.Models.Core;
using BLL.Models.Core.Doctor;
using DAL.Entities.Core;

namespace BLL.Services.Abstraction.Core
{
    public interface IDoctorServices
    {
        Task<(bool IsSuccess, List<DoctorModel>? Doctors)> GetAllAsync();
        Task<(bool isSuccess, DoctorModel? Doctor)> GetByIdAsync(string id);
        Task<(bool IsSuccess, string? Error, DoctorModel? Doctor)> CreateDoctorProfileAsync(AddDoctorProfile model, string userId);
        Task<(bool IsSuccess, string? Error, DoctorModel? Doctor)> UpdateDoctorProfileAsync(EditDoctorProfile model, string userId);
        Task<(bool IsSuccess, string? Error)> RespondToSupervisionRequestAsync(string requestId, bool accept, string doctorUserId);
        Task<(bool IsSuccess, List<SupervisionRequest>? Requests)> GetSupervisionRequestsAsync(string doctorUserId);
        Task<(bool IsSuccess, string? Error)> RemoveSupervisionOnPatient(string patientUserId);
        Task<(bool IsSuccess, List<DoctorRatingDTO>? Ratings)> GetDoctorRatingsAsync(string doctorId);
    }
}
