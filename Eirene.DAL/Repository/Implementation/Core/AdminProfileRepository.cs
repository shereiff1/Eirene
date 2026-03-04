using DAL.Database;
using DAL.Entities.Core;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Core;

internal class AdminProfileRepository : GenericRepository<AdminProfile>, IAdminProfileRepository
{
    public AdminProfileRepository(EireneDBContext context) : base(context)
    {
    }
    public override async Task<List<AdminProfile>> GetAllAsync()
    {
        return await _context.Set<AdminProfile>()
            .Include(x => x.User)
            .ToListAsync();
    }

    public override async Task<AdminProfile?> GetByIdAsync(object id)
    {
        return await _context.Set<AdminProfile>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == (string)id);
    }
}
