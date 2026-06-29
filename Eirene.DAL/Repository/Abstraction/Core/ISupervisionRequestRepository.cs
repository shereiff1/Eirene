using Eirene.DAL.Entities.Core;

namespace Eirene.DAL.Repository.Abstraction.Core;

    public interface ISupervisionRequestRepository : IGenericRepository<SupervisionRequest>
    {
        Task<List<SupervisionRequest>> GetDoctorPatientsAsync(string doctorId);
        Task<(List<SupervisionRequest> Items, int TotalCount)> GetDoctorPatientsPagedAsync(string doctorId, int page, int pageSize);
        Task<List<SupervisionRequest>> GetRequestsByDoctorIdAsync(string doctorId);
    }
