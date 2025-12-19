using DAL.Database;
using DAL.Entities.Tracking;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Tracking;

namespace DAL.Repository.Implementation.Tracking;

internal class MoodTrackerRepository : GenericRepository<MoodTracker>, IMoodTrackerRepository
{
    public MoodTrackerRepository(EireneDBContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
}
