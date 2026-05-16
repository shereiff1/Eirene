using Eirene.DAL.Entities.Core;

namespace Eirene.DAL.Repository.Abstraction.Core;

public interface ISupervisionRequestRepository : IGenericRepository<SupervisionRequest>
{
    Task<List<SupervisionRequest>> GetDoctorPatientsAsync(string doctorId);
}
