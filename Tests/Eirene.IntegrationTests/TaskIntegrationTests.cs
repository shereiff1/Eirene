using System.Net;
using System.Net.Http.Json;
using Eirene.BLL.Models.Core.Patient;
using Eirene.BLL.Models.Treatment.Task;
using Eirene.DAL.Database;
using Eirene.DAL.Entities.Treatment;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Eirene.IntegrationTests;

public class TaskIntegrationTests : BaseIntegrationTest
{
    public TaskIntegrationTests(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task FullTaskFlow_Analysis_Generation_Retrieval_Update()
    {
        // 1. Setup Patient
        var (_, userId) = await AuthenticateAsync("patient@tasks.com", "Patient");
        await Client.PostAsJsonAsync("/api/Patient/profile", new AddPatientProfile
        {
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "Task St",
            EmergencyContact = "+1234567890",
            PhoneNumber = "+1234567890",
            MedicalHistory = "None"
        });

        // 2. Seed Answers in DB
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
            var question = new Question { Id = Guid.NewGuid(), QuestionContent = "How do you feel?" };
            db.Questions.Add(question);
            db.QuestionAnswers.Add(new QuestionAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Answer = "I feel anxious and tired.",
                PatientId = userId
            });
            await db.SaveChangesAsync();
        }

        // 3. Mock AI Model Response
        var mockResponse = "{\"dominant_condition\": \"Anxiety\", \"confidence_level\": \"High\", \"problems\": [\"Stress\"], \"tasks_for_user\": [{\"task\": \"Breathe deeply\", \"rationale\": \"Calm down\", \"difficulty\": \"Easy\"}]}";
        Factory.AIModelServiceMock.Setup(x => x.AnalyzeUserAnswersAsync(It.IsAny<string>()))
            .ReturnsAsync(mockResponse);

        // 4. Trigger Analysis
        var analyzeResponse = await Client.GetAsync("/api/Diagnosis/analyze");
        var content = await analyzeResponse.Content.ReadAsStringAsync();
        analyzeResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"Content: {content}");

        // 5. Verify Tasks Generated and Retrievable
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
            var allTasks = await db.PatientTasks.ToListAsync();
            var allPlans = await db.TreatmentPlans.ToListAsync();
            var user = await db.Users.Include(u => u.PatientProfile).FirstOrDefaultAsync(u => u.Id == userId);
            // allTasks.Should().NotBeEmpty("Tasks should have been added to the database");
        }

        var getTasksResponse = await Client.GetAsync("/api/PatientTask/user");
        getTasksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tasks = await getTasksResponse.Content.ReadFromJsonAsync<List<PatientTaskResponseDTO>>();
        Assert.NotNull(tasks);
        tasks.Should().ContainSingle(t => t.Description == "Breathe deeply");
        var taskId = tasks.First().Id;

        // 6. Update Task Status
        var updateResponse = await Client.PutAsJsonAsync($"/api/PatientTask/{taskId}/status", new { IsCompleted = true });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 7. Verify in DB
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EireneDBContext>();
            var task = await db.PatientTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            Assert.NotNull(task);
            task.IsCompleted.Should().BeTrue();
        }
    }
}
