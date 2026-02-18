 

using BLL.Models.Core.Patient;

namespace BLL.Services.Abstraction.Core
{
    public interface IPatientServices
    {
        Task<(bool IsSuccess, string? Error)> AssignDoctorAsync(string patientUserId, string doctorId);
        // Task<(bool IsSuccess, string? Error)> CreatePatientProfileAsync (AddPatientProfile model, string patientId);
        
    }
}
