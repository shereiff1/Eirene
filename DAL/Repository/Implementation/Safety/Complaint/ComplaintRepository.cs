using DAL.Repository.Abstraction.Safety.Complaint;
using ComplaintEntity = DAL.Entities.Safety.Complaint.Complaint;

namespace DAL.Repository.Implementation.Safety.Complaint;

internal class ComplaintRepository : GenericRepository<ComplaintEntity>, IComplaintRepository
{

}
