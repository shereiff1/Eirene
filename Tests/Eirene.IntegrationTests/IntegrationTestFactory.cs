using Eirene.BLL.AIModel.Abstraction;
using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Eirene.DAL.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace Eirene.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Mock<IAIModelService> AIModelServiceMock { get; } = new();
    public Mock<IBackgroundJobService> BackgroundJobServiceMock { get; } = new();

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("eirene_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public string DbConnectionString => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        // This is the earliest we can set the configuration for the TestHost
        builder.UseConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["Redis:ConnectionString"] = _redisContainer.GetConnectionString(),
                ["JwtSettings:Secret"] = "super_secret_key_for_testing_purposes_only_12345",
                ["JwtSettings:Issuer"] = "EireneTest",
                ["JwtSettings:Audience"] = "EireneTestUsers",
                ["CloudinarySettings:CloudName"] = "test-cloud",
                ["CloudinarySettings:ApiKey"] = "test-key",
                ["CloudinarySettings:ApiSecret"] = "test-secret"
            })
            .Build());

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext
            services.RemoveAll(typeof(DbContextOptions<EireneDBContext>));

            // Add Test DbContext
            services.AddDbContext<EireneDBContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.ConfigureWarnings(x => x.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            // Mock external AI service
            services.RemoveAll(typeof(IAIModelService));
            services.AddSingleton(AIModelServiceMock.Object);

            // Mock Background Job Service to avoid Hangfire connection issues
            services.RemoveAll(typeof(IBackgroundJobService));
            services.AddSingleton(BackgroundJobServiceMock.Object);
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        
        // Ensure database is created
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
        
        // Use MigrateAsync to ensure all tables (including Identity) are created
        await db.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Patient", "Doctor", "Moderator", "Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
}
