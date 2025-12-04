using DAL.Entities.Core;
using DAL.Repository.Abstraction.Core;
using DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;
using DAL.Database;

namespace DAL.Repository.Implementation.Core
{
    public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(EireneDBContext context, IUnitOfWork unitOfWork)
            : base(context, unitOfWork)
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