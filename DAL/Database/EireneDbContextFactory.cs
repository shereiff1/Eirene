using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL.Database
{
    public class EireneDbContextFactory : IDesignTimeDbContextFactory<EireneDBContext>
    {
        public EireneDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EireneDBContext>();
            optionsBuilder.UseSqlServer("Server=SHERIF\\SQLEXPRESS;Database=EireneDB;Trusted_Connection=True;MultipleActiveResultsets=true;TrustServerCertificate=true");

            return new EireneDBContext(optionsBuilder.Options);
        }
    }
}
