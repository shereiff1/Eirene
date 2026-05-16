using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Core;
using Eirene.DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;
using Eirene.DAL.Database;

namespace Eirene.DAL.Repository.Implementation.Core
{
    public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(EireneDBContext context)
            : base(context)
        {
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _context.Set<ApplicationUser>()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetUserWithProfilesAsync(string userId)
        {
            return await _context.Set<ApplicationUser>()
                .Include(u => u.DoctorProfile)
                .Include(u => u.PatientProfile)
                .Include(u => u.ModeratorProfile)
                .Include(u => u.AdminProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }


        public async Task<List<ApplicationUser>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Set<ApplicationUser>()
                .ToListAsync();
        }
    }
}
