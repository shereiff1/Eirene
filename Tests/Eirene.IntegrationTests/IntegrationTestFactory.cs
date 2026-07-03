using Eirene.BLL.AIModel.Abstraction;
using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Eirene.DAL.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Eirene.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Mock<IAIModelService> AIModelServiceMock { get; } = new();
    public Mock<IBackgroundJobService> BackgroundJobServiceMock { get; } = new();

    // Keep the connection alive for the factory lifetime so the in-memory SQLite DB persists
    private readonly SqliteConnection _keepAliveConnection = new("DataSource=:memory:");

    static IntegrationTestFactory()
    {
        Environment.SetEnvironmentVariable("Storage__Provider", "Local");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Supply configuration values.
        builder.ConfigureAppConfiguration((context, conf) =>
        {
            conf.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=eirene_test;Username=postgres;Password=postgres",
                ["Redis:ConnectionString"] = "localhost:6379",
                ["JwtSettings:Secret"] = "super_secret_key_for_testing_purposes_only_12345",
                ["JwtSettings:Issuer"] = "EireneTest",
                ["JwtSettings:Audience"] = "EireneTestUsers",
                ["Storage:Provider"] = "Local"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Program.cs skips AddDbContextPool in Testing, so we just register our SQLite context.
            // Open a persistent connection so the :memory: database survives across DbContext scopes.
            _keepAliveConnection.Open();

            services.AddDbContext<EireneDBContext>(options =>
            {
                options.UseSqlite(_keepAliveConnection);
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            // Replace Redis distributed cache with an in-memory implementation
            services.RemoveAll(typeof(IDistributedCache));
            services.AddDistributedMemoryCache();

            // Mock external AI service
            services.RemoveAll(typeof(IAIModelService));
            services.AddSingleton(AIModelServiceMock.Object);

            // Mock Background Job Service to avoid any job-scheduling side-effects
            services.RemoveAll(typeof(IBackgroundJobService));
            services.AddSingleton(BackgroundJobServiceMock.Object);

            // Mock Storage Services to avoid needing real Cloudinary credentials
            var mockPictureService = new Mock<Eirene.BLL.Services.Abstraction.Core.IPictureService>();
            mockPictureService
                .Setup(s => s.UploadPictureAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
                .ReturnsAsync((true, "http://dummy-url.com/picture.jpg", null));
            services.RemoveAll(typeof(Eirene.BLL.Services.Abstraction.Core.IPictureService));
            services.AddSingleton<Eirene.BLL.Services.Abstraction.Core.IPictureService>(mockPictureService.Object);

            var mockDocService = new Mock<Eirene.BLL.Services.Abstraction.Core.IDocumentStorageService>();
            services.RemoveAll(typeof(Eirene.BLL.Services.Abstraction.Core.IDocumentStorageService));
            services.AddSingleton<Eirene.BLL.Services.Abstraction.Core.IDocumentStorageService>(mockDocService.Object);
        });
    }

    public async Task InitializeAsync()
    {
        // Build the SQLite schema and seed required roles
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Patient", "Doctor", "Moderator", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    public new async Task DisposeAsync()
    {
        await _keepAliveConnection.CloseAsync();
        _keepAliveConnection.Dispose();
    }
}
