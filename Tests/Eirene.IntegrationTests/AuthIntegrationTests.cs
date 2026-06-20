using System.Net;
using System.Net.Http.Json;
using Eirene.BLL.Models.Identity;
using Eirene.DAL.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eirene.IntegrationTests;

public class AuthIntegrationTests : BaseIntegrationTest
{
    public AuthIntegrationTests(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ValidPatient_ReturnsOk_AndCreatesUserInDb()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Email = "patient@test.com",
            UserName = "patient_test",
            FullName = "Test Patient",
            Password = "Password123!",
            Gender = "Male",
            Role = "Patient"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/Register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        Assert.NotNull(user);
        user.UserName.Should().Be(registerDto.UserName);
        user.FullName.Should().Be(registerDto.FullName);
        user.IsEmailVerified.Should().BeFalse();
        user.EmailVerificationCode.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_ValidCredentials_AfterEmailConfirmation_ReturnsToken()
    {
        // Arrange
        var email = "login@test.com";
        var password = "Password123!";
        var registerDto = new RegisterDTO
        {
            Email = email,
            UserName = "login_test",
            FullName = "Login Test",
            Password = password,
            Gender = "Female",
            Role = "Patient"
        };

        await Client.PostAsJsonAsync("/api/Auth/Register", registerDto);

        // Manually confirm email in DB
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
            var user = await db.Users.FirstAsync(u => u.Email == email);
            user.EmailConfirmed = true;
            user.IsEmailVerified = true;
            await db.SaveChangesAsync();
        }

        var loginDto = new LoginDTO
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/Login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        Assert.NotNull(authResult);
        authResult.Success.Should().BeTrue();
        authResult.AccessToken.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Login_WithoutEmailConfirmation_ReturnsFailure()
    {
        // Arrange
        var email = "unconfirmed@test.com";
        var password = "Password123!";
        var registerDto = new RegisterDTO
        {
            Email = email,
            UserName = "unconfirmed_test",
            FullName = "Unconfirmed Test",
            Password = password,
            Gender = "Other",
            Role = "Patient"
        };

        await Client.PostAsJsonAsync("/api/Auth/Register", registerDto);

        var loginDto = new LoginDTO
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/Login", loginDto);

        // Assert
        // Depending on implementation, it might return Unauthorized or Ok with IsSuccess = false
        var authResult = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        authResult!.Success.Should().BeFalse();
    }
}
