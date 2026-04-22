using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Eirene.DAL.Database
{
    public class EireneDbContextFactory : IDesignTimeDbContextFactory<EireneDBContext>
    {
        public EireneDBContext CreateDbContext(string[] args)
        {
            var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Eirene.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddUserSecrets("5601635f-8108-415a-8b30-84f7b2c2d6dc", reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found in appsettings, user secrets, or environment variables.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<EireneDBContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new EireneDBContext(optionsBuilder.Options);
        }
    }
}
