using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Core;

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
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public override async Task<SupervisionRequest?> GetByIdAsync(object id)
    {
        return await _context.Set<SupervisionRequest>()
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .Include(r => r.Doctor)
                .ThenInclude(d => d.User)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == (string)id);
    }
    public async Task<List<SupervisionRequest>> GetDoctorPatientsAsync(string doctorId)
    {
        return await _context.Set<SupervisionRequest>()
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .Where(r => r.DoctorProfileId == doctorId && r.Status == Eirene.DAL.Enumerators.SupervisionRequestStatus.Accepted)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<SupervisionRequest> Items, int TotalCount)> GetDoctorPatientsPagedAsync(string doctorId, int page, int pageSize)
    {
        var query = _context.Set<SupervisionRequest>()
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .Where(r => r.DoctorProfileId == doctorId && r.Status == Eirene.DAL.Enumerators.SupervisionRequestStatus.Accepted)
            .AsSplitQuery()
            .AsNoTracking();
            
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        
        return (items, totalCount);
    }

    public async Task<List<SupervisionRequest>> GetRequestsByDoctorIdAsync(string doctorId)
    {
        return await _context.Set<SupervisionRequest>()
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .Where(r => r.DoctorProfileId == doctorId)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }
}
