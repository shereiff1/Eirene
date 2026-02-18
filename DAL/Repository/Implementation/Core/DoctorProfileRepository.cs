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

    public override async Task<List<DoctorProfile>> GetAllAsync()
    {
        return await _context.Set<DoctorProfile>()
            .Include(x => x.User)
            .Include(x => x.Patients)
            .ToListAsync();
    }

    public override async Task<DoctorProfile?> GetByIdAsync(object id)
    {
        return await _context.Set<DoctorProfile>()
            .Include(x => x.User)
            .Include(x => x.Patients)
            .FirstOrDefaultAsync(x => x.UserId == (string)id);
    }
}
