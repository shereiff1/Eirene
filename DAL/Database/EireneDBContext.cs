using DAL.Entities.Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Database;

public class EireneDBContext : IdentityDbContext<ApplicationUser>
{
    public EireneDBContext(DbContextOptions<EireneDBContext> options)
        : base(options)
    {
    }
}