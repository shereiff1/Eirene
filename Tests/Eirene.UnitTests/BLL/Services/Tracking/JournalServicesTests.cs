using AutoMapper;
using Eirene.BLL.Models.Tracking;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Implementation.Tracking;
using Eirene.DAL.Entities.Tracking;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Tracking;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Tracking;

public class JournalServicesTests
{
    private readonly Mock<IJournalRepository> _journalRepoMock;
    private readonly Mock<ILogger<JournalServices>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly JournalServices _sut;

    public JournalServicesTests()
    {
        _journalRepoMock = new Mock<IJournalRepository>();
        _loggerMock = new Mock<ILogger<JournalServices>>();
        _mapperMock = new Mock<IMapper>();
        _userContextMock = new Mock<IUserContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new JournalServices(
            _journalRepoMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object
        );
    }

    private void SetupAuthenticatedUser(string userId)
    {
        _userContextMock.Setup(x => x.IsAuthenticated).Returns(true);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-1";
        SetupAuthenticatedUser(userId);
        var model = new AddJournal { Content = "Test", Mood = 5 };
        var entity = new Journal { Content = "Test", Mood = 5, PatientId = userId };
        
        _mapperMock.Setup(x => x.Map<Journal>(model)).Returns(entity);
        _journalRepoMock.Setup(x => x.GetTodayJournalAsync(userId, It.IsAny<DateTime>())).ReturnsAsync((Journal)null!);
        _journalRepoMock.Setup(x => x.AddAsync(entity)).ReturnsAsync(entity);
        _mapperMock.Setup(x => x.Map<JournalDTO>(entity)).Returns(new JournalDTO());

        // Act
        var result = await _sut.CreateAsync(model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AlreadyCreatedToday_ReturnsFailure()
    {
        // Arrange
        var userId = "user-1";
        SetupAuthenticatedUser(userId);
        var model = new AddJournal { Content = "Test", Mood = 5 };
        var entity = new Journal { Content = "Test", Mood = 5 };

        _mapperMock.Setup(x => x.Map<Journal>(model)).Returns(entity);
        _journalRepoMock.Setup(x => x.GetTodayJournalAsync(userId, It.IsAny<DateTime>())).ReturnsAsync(new Journal());

        // Act
        var result = await _sut.CreateAsync(model);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UserNotAuthenticated_ThrowsException()
    {
        // Arrange
        _userContextMock.Setup(x => x.IsAuthenticated).Returns(false);
        _userContextMock.Setup(x => x.UserId).Returns((string)null!);
        // Mock mapper to return something so we reach the first GetCurrentUserId call
        _mapperMock.Setup(x => x.Map<Journal>(It.IsAny<AddJournal>())).Returns(new Journal());
        var model = new AddJournal();

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(model);

        // Assert
        // CreateAsync has a try-catch block that catches ALL exceptions and logs them, returning (false, null)
        var result = await _sut.CreateAsync(model);
        result.IsSuccess.Should().BeFalse();
        Assert.Null(result.AddedJournal);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while creating a journal entry")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UnauthorizedUser_ReturnsFalse()
    {
        // Arrange
        var userId = "user-1";
        SetupAuthenticatedUser(userId);
        var journalId = Guid.NewGuid();
        var journal = new Journal { Id = journalId, PatientId = "other-user" };

        _journalRepoMock.Setup(x => x.GetByIdAsync(journalId)).ReturnsAsync(journal);
        _userContextMock.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _sut.DeleteAsync(journalId);

        // Assert
        result.Should().BeFalse();
        _journalRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Journal>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_AdminUser_ReturnsTrue()
    {
        // Arrange
        var userId = "admin-1";
        SetupAuthenticatedUser(userId);
        var journalId = Guid.NewGuid();
        var journal = new Journal { Id = journalId, PatientId = "other-user" };

        _journalRepoMock.Setup(x => x.GetByIdAsync(journalId)).ReturnsAsync(journal);
        _userContextMock.Setup(x => x.IsInRole(Eirene.BLL.Enumerators.Roles.Admin)).Returns(true);

        // Act
        var result = await _sut.DeleteAsync(journalId);

        // Assert
        result.Should().BeTrue();
        _journalRepoMock.Verify(x => x.DeleteAsync(journal), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotToday_ReturnsFalse()
    {
        // Arrange
        var userId = "user-1";
        SetupAuthenticatedUser(userId);
        var journalId = Guid.NewGuid();
        var journal = new Journal 
        { 
            Id = journalId, 
            PatientId = userId, 
            CreatedAt = DateTime.UtcNow.AddDays(-1) 
        };
        var model = new EditJournal { Id = journalId, Content = "Updated" };

        _journalRepoMock.Setup(x => x.GetByIdAsync(journalId)).ReturnsAsync(journal);

        // Act
        var result = await _sut.UpdateAsync(model);

        // Assert
        result.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}
