using DAL.Database;
using DAL.Entities.Core;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Core;

internal class PatientProfileRepository :  GenericRepository<PatientProfile>, IPatientProfileRepository
{
    public PatientProfileRepository(EireneDBContext context) : base(context)
    {
    }
}
