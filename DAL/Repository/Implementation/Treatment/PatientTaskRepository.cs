using DAL.Database;
using DAL.Repository.Abstraction.Treatment;
using PatientTask = DAL.Entities.Treatment.PatientTask;

namespace DAL.Repository.Implementation.Treatment
{
    public class PatientTaskRepository : GenericRepository<PatientTask>, IPatientTaskRepository
    {
        public PatientTaskRepository(EireneDBContext context) : base(context)
        {
        }
    }
}
