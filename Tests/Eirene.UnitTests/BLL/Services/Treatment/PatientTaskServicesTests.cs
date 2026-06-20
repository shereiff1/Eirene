using AutoFixture;
using AutoFixture.AutoMoq;
using Eirene.BLL.Models.Model_Result;
using Eirene.BLL.Models.Treatment.Task;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Treatment;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Treatment;

public class PatientTaskServicesTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPatientTaskRepository> _taskRepoMock;
    private readonly Mock<ITreatmentPlanRepository> _treatmentPlanRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<ILogger<PatientTaskServices>> _loggerMock;
    private readonly PatientTaskServices _sut;

    public PatientTaskServicesTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _taskRepoMock = new Mock<IPatientTaskRepository>();
        _treatmentPlanRepoMock = new Mock<ITreatmentPlanRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<ILogger<PatientTaskServices>>();

        _sut = new PatientTaskServices(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _taskRepoMock.Object,
            _treatmentPlanRepoMock.Object,
            _userContextMock.Object);
    }

    // ========== AddTasksFromModelAsync ==========

    [Fact]
    public async Task AddTasksFromModelAsync_ValidTasks_CreatesTasksAndReturnsTrue()
    {
        // Arrange
        var userId = "user-1";
        var modelResult = new AITaskResponse
        {
            TasksForUser = new List<TaskItem>
            {
                new TaskItem { Task = "Go for a walk" },
                new TaskItem { Task = "Practice mindfulness" },
                new TaskItem { Task = "Write in journal" }
            }
        };

        // Act
        var result = await _sut.AddTasksFromModelAsync(modelResult, userId);

        // Assert
        result.Should().BeTrue();
        _treatmentPlanRepoMock.Verify(x => x.AddAsync(It.Is<TreatmentPlan>(tp => tp.UserId == userId)), Times.Once);
        _taskRepoMock.Verify(x => x.AddAsync(It.IsAny<PatientTask>()), Times.Exactly(3));
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2)); // Once for plan, once for tasks
    }

    [Fact]
    public async Task AddTasksFromModelAsync_NullTasksForUser_ReturnsFalse()
    {
        // Arrange
        var modelResult = new AITaskResponse { TasksForUser = null! };

        // Act
        var result = await _sut.AddTasksFromModelAsync(modelResult, "user-1");

        // Assert
        result.Should().BeFalse();
        _treatmentPlanRepoMock.Verify(x => x.AddAsync(It.IsAny<TreatmentPlan>()), Times.Never);
    }

    [Fact]
    public async Task AddTasksFromModelAsync_EmptyTaskList_ReturnsFalse()
    {
        // Arrange
        var modelResult = new AITaskResponse { TasksForUser = new List<TaskItem>() };

        // Act
        var result = await _sut.AddTasksFromModelAsync(modelResult, "user-1");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddTasksFromModelAsync_OnlyWhitespaceTasks_ReturnsFalse()
    {
        // Arrange
        var modelResult = new AITaskResponse
        {
            TasksForUser = new List<TaskItem>
            {
                new TaskItem { Task = "   " },
                new TaskItem { Task = "" },
                new TaskItem { Task = null! }
            }
        };

        // Act
        var result = await _sut.AddTasksFromModelAsync(modelResult, "user-1");

        // Assert
        result.Should().BeFalse();
    }

    // ========== UpdateTaskStatusAsync ==========

    [Fact]
    public async Task UpdateTaskStatusAsync_ValidOwner_UpdatesAndReturnsTrue()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = "user-1";
        var task = new PatientTask { Id = taskId, PatientId = userId, IsCompleted = false };

        _taskRepoMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync(task);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _taskRepoMock.Setup(x => x.UpdateAsync(task)).ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateTaskStatusAsync(taskId, true);

        // Assert
        result.Should().BeTrue();
        task.IsCompleted.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_WrongOwner_ReturnsFalse()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new PatientTask { Id = taskId, PatientId = "owner-user", IsCompleted = false };

        _taskRepoMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync(task);
        _userContextMock.Setup(x => x.UserId).Returns("different-user");

        // Act
        var result = await _sut.UpdateTaskStatusAsync(taskId, true);

        // Assert
        result.Should().BeFalse();
        _taskRepoMock.Verify(x => x.UpdateAsync(It.IsAny<PatientTask>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_TaskNotFound_ReturnsFalse()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepoMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync((PatientTask?)null);

        // Act
        var result = await _sut.UpdateTaskStatusAsync(taskId, true);

        // Assert
        result.Should().BeFalse();
    }

    // ========== GetTaskByIdAsync ==========

    [Fact]
    public async Task GetTaskByIdAsync_ValidTask_ReturnsDto()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new PatientTask
        {
            Id = taskId,
            Description = "Test task",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            PatientId = "user-1"
        };

        _taskRepoMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync(task);

        // Act
        var result = await _sut.GetTaskByIdAsync(taskId);

        // Assert
        Assert.NotNull(result);
        result.Id.Should().Be(taskId);
        result.Description.Should().Be("Test task");
    }

    [Fact]
    public async Task GetTaskByIdAsync_TaskNotFound_ReturnsNull()
    {
        // Arrange
        _taskRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PatientTask?)null);

        // Act
        var result = await _sut.GetTaskByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    // ========== GetTasksForUserAsync ==========

    [Fact]
    public async Task GetTasksForUserAsync_WithTasks_ReturnsOrderedByCreatedAtDesc()
    {
        // Arrange
        var userId = "user-1";
        var tasks = new List<PatientTask>
        {
            new PatientTask { Id = Guid.NewGuid(), PatientId = userId, Description = "Old", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new PatientTask { Id = Guid.NewGuid(), PatientId = userId, Description = "New", CreatedAt = DateTime.UtcNow }
        };

        _taskRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<PatientTask, bool>>>())).ReturnsAsync(tasks);

        // Act
        var result = (await _sut.GetTasksForUserAsync(userId)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.First().Description.Should().Be("New");
        result.Last().Description.Should().Be("Old");
    }
}
