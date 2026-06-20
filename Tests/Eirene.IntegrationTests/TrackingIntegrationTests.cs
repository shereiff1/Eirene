using System.Net;
using System.Net.Http.Json;
using Eirene.BLL.Models.Core.Patient;
using Eirene.BLL.Models.Tracking;
using Eirene.DAL.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eirene.IntegrationTests;

public class TrackingIntegrationTests : BaseIntegrationTest
{
    public TrackingIntegrationTests(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task JournalFlow_CreatesAndRetrieves()
    {
        // 1. Setup Patient
        var (_, userId) = await AuthenticateAsync("patient@tracking.com", "Patient");
        await Client.PostAsJsonAsync("/api/Patient/profile", new AddPatientProfile
        {
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "Tracking St",
            EmergencyContact = "+1234567890",
            PhoneNumber = "+1234567890",
            MedicalHistory = "None"
        });

        // 2. Create Journal Entry
        var addJournal = new AddJournal
        {
            Content = "Today I feel much better after the exercise.",
            Mood = 8
        };
        var journalResponse = await Client.PostAsJsonAsync("/api/Journal", addJournal);
        journalResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 4. Retrieve Journal Entries
        var getJournalsResponse = await Client.GetAsync("/api/Journal");
        getJournalsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var journals = await getJournalsResponse.Content.ReadFromJsonAsync<List<JournalDTO>>();
        journals.Should().ContainSingle(j => j.Content == addJournal.Content);

        // 6. Verify in DB
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
        
        var dbJournal = await db.Journals.FirstOrDefaultAsync(j => j.PatientId == userId);
        Assert.NotNull(dbJournal);
        dbJournal.Content.Should().Be(addJournal.Content);
    }
}
