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
}
