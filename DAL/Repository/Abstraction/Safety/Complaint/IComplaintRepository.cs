using ComplaintEntity = DAL.Entities.Safety.Complaint.Complaint;

namespace DAL.Repository.Abstraction.Safety.Complaint;

public interface IComplaintRepository : IGenericRepository<ComplaintEntity>
{
}
