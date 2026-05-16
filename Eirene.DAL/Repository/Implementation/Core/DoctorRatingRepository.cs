using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Core;

namespace Eirene.DAL.Repository.Implementation.Core
{
    public class DoctorRatingRepository : GenericRepository<DoctorRating>, IDoctorRatingRepository
    {
        public DoctorRatingRepository(EireneDBContext dbContext) : base(dbContext)
        {
        }
    }
}
