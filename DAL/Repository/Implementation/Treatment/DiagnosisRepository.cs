using DAL.Entities.Treatment;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Treatment;

namespace DAL.Repository.Implementation.Treatment;

internal class DiagnosisRepository : GenericRepository<Diagnosis>, IDiagnosisRepository
{
}