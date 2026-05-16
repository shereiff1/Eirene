using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.DAL.Entities.Core;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface ISupervisionService
    {
        Task<Result> RespondToSupervisionRequestAsync(string requestId, bool accept, string doctorUserId);
        Task<Result<List<SupervisionRequest>>> GetSupervisionRequestsAsync(string doctorUserId);
        Task<Result<List<DoctorPatientDTO>>> GetDoctorsPatientsAsync(string doctorUserId);
        Task<Result> RemoveSupervisionOnPatient(string patientUserId);
    }
}
