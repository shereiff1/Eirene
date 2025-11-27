using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Safety.Complaint;
using Microsoft.EntityFrameworkCore;
using ComplaintEntity = DAL.Entities.Safety.Complaint.Complaint;

namespace DAL.Repository.Implementation.Safety.Complaint;

internal class ComplaintRepository : GenericRepository<ComplaintEntity>, IComplaintRepository
{
    public ComplaintRepository(DbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
}
