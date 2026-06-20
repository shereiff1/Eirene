using System.Net;
using System.Net.Http.Json;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Models.Core.Patient;
using Eirene.DAL.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eirene.IntegrationTests;

public class SupervisionIntegrationTests : BaseIntegrationTest
{
    public SupervisionIntegrationTests(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CompleteSupervisionFlow_Registers_Requests_Accepts()
    {
        // 1. Register and Create Doctor Profile
        var (_, doctorUserId) = await AuthenticateAsync("doctor@flow.com", "Doctor");
        var addDoctorProfile = new AddDoctorProfile
        {
            Specialization = "Psychiatry",
            Biography = "Experienced psychiatrist with over 10 years of experience in the field. I specialize in anxiety and depression treatment.",
            Qualifications = "MD, PhD",
            YearsOfExperience = 10,
            PhoneNumber = "+1987654321"
        };
        var docProfileResponse = await Client.PostAsJsonAsync("/api/Doctor/profile", addDoctorProfile);
        docProfileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Manually verify doctor in DB to bypass filter
        using (var innerScope = Factory.Services.CreateScope())
        {
            var innerDb = innerScope.ServiceProvider.GetRequiredService<EireneDBContext>();
            var doctor = await innerDb.DoctorProfiles.FirstAsync(d => d.Id == doctorUserId);
            doctor.IsVerified = true;
            await innerDb.SaveChangesAsync();
        }

        // 2. Register and Create Patient Profile
        var (_, patientUserId) = await AuthenticateAsync("patient@flow.com", "Patient");
        var addPatientProfile = new AddPatientProfile
        {
            DateOfBirth = new DateTime(1995, 5, 5),
            Address = "456 Flow St",
            EmergencyContact = "+1122334455",
            PhoneNumber = "+1122334455",
            MedicalHistory = "Chronic anxiety"
        };
        var patProfileResponse = await Client.PostAsJsonAsync("/api/Patient/profile", addPatientProfile);
        patProfileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Patient Requests Supervision
        var requestResponse = await Client.PutAsync($"/api/Patient/request-doctor/{doctorUserId}", null);
        requestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Doctor Login and Get Requests
        await AuthenticateAsync("doctor@flow.com", "Doctor");
        var getRequestsResponse = await Client.GetAsync("/api/Doctor/supervision-requests");
        getRequestsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var requests = await getRequestsResponse.Content.ReadFromJsonAsync<List<SupervisionRequestDTO>>();
        requests.Should().NotBeNull();
        requests!.Should().Contain(r => r.PatientProfileId == patientUserId);
        var requestId = requests.First(r => r.PatientProfileId == patientUserId).Id;

        // 5. Doctor Accepts Request
        var acceptResponse = await Client.PutAsJsonAsync($"/api/Doctor/supervision-requests/{requestId}", true);
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Verify Assignment in DB
        using var finalScope = Factory.Services.CreateScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<EireneDBContext>();
        var patient = await finalDb.PatientProfiles.Include(p => p.Doctor).FirstOrDefaultAsync(p => p.Id == patientUserId);
        
        Assert.NotNull(patient);
        patient.DoctorProfileId.Should().Be(doctorUserId);
    }
}
