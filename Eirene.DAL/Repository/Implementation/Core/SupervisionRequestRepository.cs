using DAL.Database;
using DAL.Entities.Core;
using DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Core;

internal class SupervisionRequestRepository : GenericRepository<SupervisionRequest>, ISupervisionRequestRepository
{
    public SupervisionRequestRepository(EireneDBContext context) : base(context)
    {
    }

    public override async Task<List<SupervisionRequest>> GetAllAsync()
    {
        return await _context.Set<SupervisionRequest>()
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .Include(r => r.Doctor)
                .ThenInclude(d => d.User)
            .ToListAsync();
    }

    public override async Task<SupervisionRequest?> GetByIdAsync(object id)
    {
        return await _context.Set<SupervisionRequest>()
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .Include(r => r.Doctor)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(r => r.Id == (string)id);
    }
}
