
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Eirene.DAL.Database;
 

namespace Eirene.DAL.Repository.Implementation.Treatment;

public class TreatmentPlanRepository : GenericRepository<TreatmentPlan>, ITreatmentPlanRepository
{
    public TreatmentPlanRepository(EireneDBContext context) : base(context)
    {
    }
}
