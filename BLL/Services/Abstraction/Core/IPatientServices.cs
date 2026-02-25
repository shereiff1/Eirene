 

using BLL.Models.Core.Patient;

namespace BLL.Services.Abstraction.Core
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
    }
}
