using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Core;

namespace Eirene.DAL.Repository.Implementation.Core
{
    public class DoctorDocumentRepository : GenericRepository<DoctorDocument>, IDoctorDocumentRepository
    {
        public DoctorDocumentRepository(EireneDBContext context) : base(context)
        {
        }
    }
}
