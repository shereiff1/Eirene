using Eirene.DAL.Database;
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction.Treatment;

namespace Eirene.DAL.Repository.Implementation.Treatment
{
    internal class PatientTaskRepository : GenericRepository<PatientTask>, IPatientTaskRepository
    {
        public PatientTaskRepository(EireneDBContext context) : base(context)
        {
        }
    }
}
