using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;
using Eirene.DAL.Database;
using Microsoft.AspNetCore.Identity;

namespace Eirene.DAL.Repository.Implementation.Core
{
    internal class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
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
                .Where(u => _context.UserRoles
                    .Any(ur => ur.UserId == u.Id &&
                        _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == roleName)))
                .ToListAsync();
        }
    }
}
