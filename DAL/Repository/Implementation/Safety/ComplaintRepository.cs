using DAL.Entities.Safety;
using DAL.Repository.Abstraction.Safety;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Implementation.Safety
{
    public class ComplaintRepository: GenericRepository<Complaint>, IComplaintRepository
    {
    }
}
