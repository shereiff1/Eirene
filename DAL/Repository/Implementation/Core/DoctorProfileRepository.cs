using DAL.Database;
using DAL.Entities.Core;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Core;

internal class DoctorProfileRepository : GenericRepository<DoctorProfile>, IDoctorProfileRepository
{
    public DoctorProfileRepository(EireneDBContext context) : base(context)
    {
    }
}
