using System.Net;
using System.Net.Http.Json;
using Eirene.BLL.Models.Core.Patient;
using Eirene.DAL.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eirene.IntegrationTests;

public class PatientIntegrationTests : BaseIntegrationTest
{
    public PatientIntegrationTests(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePatientProfile_ValidData_ReturnsOk_AndStoresInDb()
    {
        // Arrange
        var (_, userId) = await AuthenticateAsync("patient@profile.com", "Patient");
        
        var addProfile = new AddPatientProfile
        {
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "123 Test St",
            EmergencyContact = "+1234567890",
            PhoneNumber = "+1234567890",
            MedicalHistory = "None"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/Patient/profile", addProfile);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
        var profile = await db.PatientProfiles.FirstOrDefaultAsync(p => p.Id == userId);

        Assert.NotNull(profile);
        profile.Address.Should().Be(addProfile.Address);
    }

    [Fact]
    public async Task GetById_ExistingProfile_ReturnsProfile()
    {
        // Arrange
        var (_, userId) = await AuthenticateAsync("get@profile.com", "Patient");
        
        var addProfile = new AddPatientProfile
        {
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "123 Get St",
            EmergencyContact = "+1234567890",
            PhoneNumber = "+1234567890",
            MedicalHistory = "Initial history"
        };
        await Client.PostAsJsonAsync("/api/Patient/profile", addProfile);

        // Act
        var response = await Client.GetAsync($"/api/Patient/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<PatientModel>();
        Assert.NotNull(profile);
        profile.Id.Should().Be(userId);
        profile.Address.Should().Be(addProfile.Address);
    }
}
