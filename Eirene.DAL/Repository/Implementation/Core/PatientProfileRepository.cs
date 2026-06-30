using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Core;

internal class PatientProfileRepository :  GenericRepository<PatientProfile>, IPatientProfileRepository
{
    public PatientProfileRepository(EireneDBContext context) : base(context)
    {
    }
    
    public override async Task<List<PatientProfile>> GetAllAsync()
    {
        return await _context.Set<PatientProfile>()
            .Include(x => x.User)
            .Include(x => x.Doctor)
            .AsSplitQuery()
            .ToListAsync();
    }

    public override async Task<PatientProfile?> GetByIdAsync(object id)
    {
        return await _context.Set<PatientProfile>()
            .Include(x => x.User)
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.Id == (string)id);
    }
}
