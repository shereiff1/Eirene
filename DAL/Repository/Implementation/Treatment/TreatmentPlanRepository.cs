
using DAL.Entities.Treatment;
using DAL.Repository.Abstraction.Treatment;
using DAL.Database;
using DAL.Repository.Abstraction;


namespace DAL.Repository.Implementation.Treatment;

public class TreatmentPlanRepository : GenericRepository<TreatmentPlan>, ITreatmentPlanRepository
{
    public TreatmentPlanRepository(EireneDBContext context) : base(context)
    {
    }
}
