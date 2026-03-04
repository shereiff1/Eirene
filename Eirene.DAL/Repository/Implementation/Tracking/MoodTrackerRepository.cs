using Eirene.DAL.Database;
using Eirene.DAL.Entities.Tracking;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Tracking;

namespace Eirene.DAL.Repository.Implementation.Tracking;

internal class MoodTrackerRepository : GenericRepository<MoodTracker>, IMoodTrackerRepository
{
    public MoodTrackerRepository(EireneDBContext context) : base(context)
    {
    }
}
