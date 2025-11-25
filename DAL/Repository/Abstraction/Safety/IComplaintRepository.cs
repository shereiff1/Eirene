using DAL.Entities.Content;
using DAL.Entities.Safety;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstraction.Safety
{
    public interface IComplaintRepository: IGenericRepository<Complaint>
    {
    }
}
