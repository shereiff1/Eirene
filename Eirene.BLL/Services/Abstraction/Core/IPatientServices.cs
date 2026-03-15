 

using Eirene.BLL.Models.Core.Patient;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Enumerators;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IPatientServices
    {
        Task<(bool IsSuccess, List<PatientModel>? Patients)> GetAllAsync();
        Task<(bool IsSuccess, PatientModel? Patient)> GetByIdAsync(string userId);
        Task<(bool IsSuccess, string? Error, PatientModel? Patient)> CreatePatientProfileAsync(AddPatientProfile model, string userId);
        Task<(bool IsSuccess, string? Error, PatientModel? Patient)> UpdatePatientProfileAsync(EditPatientProfile model, string userId);
        Task<(bool IsSuccess, string? Error)> DeletePatientProfileAsync(string userId);
        Task<(bool IsSuccess, string? Error)> RequestSupervisionAsync(string patientUserId, string doctorId);
        Task<(bool IsSuccess, string? Error)> RemoveDoctorSupervision(string patientUserId);
        Task<(bool IsSuccess, string? Error)> RateSupervisorAsync(string patientUserId, string doctorId, AddDoctorRatingDTO model);
        Task<(bool IsSuccess, List<SupervisionRequest>? Requests)> GetSupervisionRequestsAsync(string patientUserId, SupervisionRequestStatus? status = null);
    }
}
