using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Eirene.DAL.Database
{
    public class EireneDbContextFactory : IDesignTimeDbContextFactory<EireneDBContext>
    {
        public EireneDBContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "EireneWebAPI"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("defaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<EireneDBContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new EireneDBContext(optionsBuilder.Options);
        }
    }
}
