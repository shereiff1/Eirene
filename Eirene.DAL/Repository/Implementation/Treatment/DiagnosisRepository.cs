using Eirene.DAL.Database;
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Treatment;

internal class DiagnosisRepository : GenericRepository<Diagnosis>, IDiagnosisRepository
{
    public DiagnosisRepository(EireneDBContext context) : base(context)
    {
    }
}
