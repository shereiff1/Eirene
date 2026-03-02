using DAL.Database;
using DAL.Entities.Core;
using DAL.Repository.Abstraction.Core;

namespace DAL.Repository.Implementation.Core
{
    public class DoctorRatingRepository : GenericRepository<DoctorRating>, IDoctorRatingRepository
    {
        public DoctorRatingRepository(EireneDBContext dbContext) : base(dbContext)
        {
        }
    }
}
