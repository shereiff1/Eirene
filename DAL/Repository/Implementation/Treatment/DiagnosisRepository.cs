using DAL.Entities.Treatment;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Treatment;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Treatment;

internal class DiagnosisRepository : GenericRepository<Diagnosis>, IDiagnosisRepository
{
    public DiagnosisRepository(DbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
}