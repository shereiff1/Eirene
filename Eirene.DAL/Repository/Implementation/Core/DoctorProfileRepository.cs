using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Core;

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
            .Include(x => x.DoctorVerification)
            .AsSplitQuery()
            .ToListAsync();
    }

    public override async Task<DoctorProfile?> GetByIdAsync(object id)
    {
        return await _context.Set<DoctorProfile>()
            .Include(x => x.User)
            .Include(x => x.Patients)
            .Include(x => x.DoctorVerification)
            .FirstOrDefaultAsync(x => x.Id == (string)id);
    }

    public override async Task<List<DoctorProfile>> FindAsync(System.Linq.Expressions.Expression<Func<DoctorProfile, bool>> predicate)
    {
        return await _context.Set<DoctorProfile>()
            .Include(x => x.User)
            .Include(x => x.Patients)
            .Include(x => x.DoctorVerification)
            .Where(predicate)
            .AsSplitQuery()
            .ToListAsync();
    }
}
