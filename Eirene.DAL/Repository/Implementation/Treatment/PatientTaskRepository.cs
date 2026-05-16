using Eirene.DAL.Database;
using Eirene.DAL.Repository.Abstraction.Treatment;
using PatientTask = Eirene.DAL.Entities.Treatment.PatientTask;

namespace Eirene.DAL.Repository.Implementation.Treatment
{
    public class PatientTaskRepository : GenericRepository<PatientTask>, IPatientTaskRepository
    {
        public PatientTaskRepository(EireneDBContext context) : base(context)
        {
        }
    }
}
