using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Core;

namespace Eirene.DAL.Repository.Implementation.Core
{
    public class DoctorVerificationRepository : GenericRepository<DoctorVerification>, IDoctorVerificationRepository
    {
        public DoctorVerificationRepository(EireneDBContext context) : base(context)
        {
        }
    }
}
