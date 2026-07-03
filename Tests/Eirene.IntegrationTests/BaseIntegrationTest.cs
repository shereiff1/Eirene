using System.Net.Http.Json;
using Eirene.BLL.Models.Identity;
using Eirene.DAL.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eirene.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    protected readonly IntegrationTestFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;

    protected BaseIntegrationTest(IntegrationTestFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
    }

    public async Task InitializeAsync()
    {
        // For SQLite in-memory: reset the database by deleting and recreating all tables.
        // This ensures test isolation without needing Respawner or a live Postgres connection.
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        // Re-seed roles after reset
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        foreach (var role in new[] { "Patient", "Doctor", "Moderator", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(role));
        }
    }

    public Task DisposeAsync()
    {
        Scope.Dispose();
        return Task.CompletedTask;
    }

    protected T GetService<T>() where T : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<T>();
    }

    protected async Task<(string Token, string UserId)> AuthenticateAsync(string email = "test@user.com", string role = "Patient")
    {
        var password = "Password123!";
        var userName = email.Split('@')[0];
        
        var registerDto = new RegisterDTO
        {
            Email = email,
            UserName = userName,
            FullName = "Authenticated User",
            Password = password,
            Gender = "Male",
            Role = role
        };

        await Client.PostAsJsonAsync("/api/Auth/Register", registerDto);

        string userId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
            var user = await db.Users.FirstAsync(u => u.Email == email);
            user.EmailConfirmed = true;
            user.IsEmailVerified = true;
            await db.SaveChangesAsync();
            userId = user.Id;
        }

        var loginDto = new LoginDTO { Email = email, Password = password };
        var response = await Client.PostAsJsonAsync("/api/Auth/Login", loginDto);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.AccessToken);
        
        return (authResult.AccessToken, userId);
    }
}
