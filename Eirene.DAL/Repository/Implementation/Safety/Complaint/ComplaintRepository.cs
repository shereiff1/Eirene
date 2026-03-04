using Eirene.DAL.Database;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Safety.Complaint;
using Microsoft.EntityFrameworkCore;
using ComplaintEntity = Eirene.DAL.Entities.Safety.Complaint.Complaint;

namespace Eirene.DAL.Repository.Implementation.Safety.Complaint;

internal class ComplaintRepository : GenericRepository<ComplaintEntity>, IComplaintRepository
{
    public ComplaintRepository(EireneDBContext context) : base(context)
    {
    }
}
