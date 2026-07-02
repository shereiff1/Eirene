using AutoFixture;
using AutoFixture.AutoMoq;
using Eirene.BLL.AIModel.Abstraction;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Communication;
using Eirene.BLL.Services.Implementation.Communication;
using Eirene.DAL.Entities.Communication;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Communication;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Communication;

public class ChatbotServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IChatbotRepository> _chatbotRepoMock;
    private readonly Mock<IChatbotApiClient> _chatbotApiClientMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ChatbotService>> _loggerMock;
    private readonly ChatbotService _sut;

    public ChatbotServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _chatbotRepoMock = new Mock<IChatbotRepository>();
        _chatbotApiClientMock = new Mock<IChatbotApiClient>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ChatbotService>>();

        _sut = new ChatbotService(
            _chatbotRepoMock.Object,
            _chatbotApiClientMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    // ========== SendMessageAsync ==========

    [Fact]
    public async Task SendMessageAsync_UserIdRequired_ReturnsFailure()
    {
        // Act
        var result = await _sut.SendMessageAsync("", new ChatbotSendMessageDto { Message = "Hello" });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User ID is required");
    }

    [Fact]
    public async Task SendMessageAsync_MessageRequired_ReturnsFailure()
    {
        // Act
        var result = await _sut.SendMessageAsync("user-1", new ChatbotSendMessageDto { Message = "" });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Message cannot be empty");
    }

    [Fact]
    public async Task SendMessageAsync_SessionNotFound_ReturnsFailure()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync((ChatbotSession)null!);

        // Act
        var result = await _sut.SendMessageAsync("user-1", new ChatbotSendMessageDto { Message = "Hello", SessionId = sessionId });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Session not found");
    }

    [Fact]
    public async Task SendMessageAsync_SessionAccessDenied_ReturnsFailure()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ChatbotSession { Id = sessionId, UserId = "other-user", IsActive = true };
        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.SendMessageAsync("user-1", new ChatbotSendMessageDto { Message = "Hello", SessionId = sessionId });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied");
    }

    [Fact]
    public async Task SendMessageAsync_SessionClosed_ReturnsFailure()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ChatbotSession { Id = sessionId, UserId = "user-1", IsActive = false };
        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.SendMessageAsync("user-1", new ChatbotSendMessageDto { Message = "Hello", SessionId = sessionId });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("closed");
    }

    [Fact]
    public async Task SendMessageAsync_NewSession_CreatesSessionAndSendsMessage()
    {
        // Arrange
        var userId = "user-1";
        var message = "Hello, chatbot!";
        var response = "Hello user!";
        var session = new ChatbotSession { Id = Guid.NewGuid(), UserId = userId, IsActive = true };
        
        _chatbotRepoMock.Setup(x => x.CreateSessionAsync(userId, null))
            .ReturnsAsync(session);
        _chatbotRepoMock.Setup(x => x.GetSessionMessagesAsync(session.Id))
            .ReturnsAsync(new List<ChatbotMessage>());
        _chatbotApiClientMock.Setup(x => x.ChatAsync(message, It.IsAny<List<ChatHistoryEntry>>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.SendMessageAsync(userId, new ChatbotSendMessageDto { Message = message });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Response.Should().Be(response);
        result.Value.SessionId.Should().Be(session.Id);

        _chatbotRepoMock.Verify(x => x.AddMessageAsync(session.Id, "user", message), Times.Once);
        _chatbotRepoMock.Verify(x => x.AddMessageAsync(session.Id, "assistant", response), Times.Once);
        _chatbotRepoMock.Verify(x => x.UpdateSessionTitleAsync(session.Id, It.IsAny<string>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendMessageAsync_ExistingSession_SendsMessageWithoutUpdatingTitle()
    {
        // Arrange
        var userId = "user-1";
        var sessionId = Guid.NewGuid();
        var message = "Follow up question";
        var response = "Response to follow up";
        var session = new ChatbotSession { Id = sessionId, UserId = userId, IsActive = true };
        var existingMessages = new List<ChatbotMessage> { new ChatbotMessage { Role = "user", Content = "Initial" } };

        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(session);
        _chatbotRepoMock.Setup(x => x.GetSessionMessagesAsync(sessionId))
            .ReturnsAsync(existingMessages);
        _chatbotApiClientMock.Setup(x => x.ChatAsync(message, It.IsAny<List<ChatHistoryEntry>>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.SendMessageAsync(userId, new ChatbotSendMessageDto { Message = message, SessionId = sessionId });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Response.Should().Be(response);

        _chatbotRepoMock.Verify(x => x.AddMessageAsync(sessionId, "user", message), Times.Once);
        _chatbotRepoMock.Verify(x => x.AddMessageAsync(sessionId, "assistant", response), Times.Once);
        _chatbotRepoMock.Verify(x => x.UpdateSessionTitleAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ApiClientReturnsEmpty_ReturnsFailure()
    {
        // Arrange
        var userId = "user-1";
        var session = new ChatbotSession { Id = Guid.NewGuid(), UserId = userId, IsActive = true };

        _chatbotRepoMock.Setup(x => x.CreateSessionAsync(userId, null))
            .ReturnsAsync(session);
        _chatbotRepoMock.Setup(x => x.GetSessionMessagesAsync(session.Id))
            .ReturnsAsync(new List<ChatbotMessage>());
        _chatbotApiClientMock.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatHistoryEntry>>()))
            .ReturnsAsync("");

        // Act
        var result = await _sut.SendMessageAsync(userId, new ChatbotSendMessageDto { Message = "Hello" });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("busy or unavailable");
    }

    // ========== GetUserSessionsAsync ==========

    [Fact]
    public async Task GetUserSessionsAsync_UserIdRequired_ReturnsFailure()
    {
        // Act
        var result = await _sut.GetUserSessionsAsync("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User ID is required");
    }

    [Fact]
    public async Task GetUserSessionsAsync_ValidUser_ReturnsSessions()
    {
        // Arrange
        var userId = "user-1";
        var sessions = new List<ChatbotSession>
        {
            new ChatbotSession { Id = Guid.NewGuid(), Title = "S1", UserId = userId },
            new ChatbotSession { Id = Guid.NewGuid(), Title = "S2", UserId = userId }
        };

        _chatbotRepoMock.Setup(x => x.GetUserSessionsAsync(userId)).ReturnsAsync(sessions);

        // Act
        var result = await _sut.GetUserSessionsAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Title.Should().Be("S1");
    }

    // ========== GetSessionMessagesAsync ==========

    [Fact]
    public async Task GetSessionMessagesAsync_UserIdRequired_ReturnsFailure()
    {
        // Act
        var result = await _sut.GetSessionMessagesAsync("", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User ID is required");
    }

    [Fact]
    public async Task GetSessionMessagesAsync_SessionNotFound_ReturnsFailure()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId)).ReturnsAsync((ChatbotSession)null!);

        // Act
        var result = await _sut.GetSessionMessagesAsync("user-1", sessionId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Session not found");
    }

    [Fact]
    public async Task GetSessionMessagesAsync_AccessDenied_ReturnsFailure()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ChatbotSession { Id = sessionId, UserId = "other-user" };
        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId)).ReturnsAsync(session);

        // Act
        var result = await _sut.GetSessionMessagesAsync("user-1", sessionId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied");
    }

    [Fact]
    public async Task GetSessionMessagesAsync_ValidRequest_ReturnsMessages()
    {
        // Arrange
        var userId = "user-1";
        var sessionId = Guid.NewGuid();
        var session = new ChatbotSession { Id = sessionId, UserId = userId };
        var messages = new List<ChatbotMessage>
        {
            new ChatbotMessage { Id = Guid.NewGuid(), Role = "user", Content = "M1" },
            new ChatbotMessage { Id = Guid.NewGuid(), Role = "assistant", Content = "M2" }
        };

        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId)).ReturnsAsync(session);
        _chatbotRepoMock.Setup(x => x.GetSessionMessagesAsync(sessionId)).ReturnsAsync(messages);

        // Act
        var result = await _sut.GetSessionMessagesAsync(userId, sessionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Content.Should().Be("M1");
    }

    // ========== DeleteSessionAsync ==========

    [Fact]
    public async Task DeleteSessionAsync_UserIdRequired_ReturnsFailure()
    {
        // Act
        var result = await _sut.DeleteSessionAsync("", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User ID is required");
    }

    [Fact]
    public async Task DeleteSessionAsync_SessionNotFound_ReturnsFailure()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId)).ReturnsAsync((ChatbotSession)null!);

        // Act
        var result = await _sut.DeleteSessionAsync("user-1", sessionId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Session not found");
    }

    [Fact]
    public async Task DeleteSessionAsync_AccessDenied_ReturnsFailure()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ChatbotSession { Id = sessionId, UserId = "other-user" };
        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId)).ReturnsAsync(session);

        // Act
        var result = await _sut.DeleteSessionAsync("user-1", sessionId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied");
    }

    [Fact]
    public async Task DeleteSessionAsync_ValidRequest_DeactivatesSession()
    {
        // Arrange
        var userId = "user-1";
        var sessionId = Guid.NewGuid();
        var session = new ChatbotSession { Id = sessionId, UserId = userId };

        _chatbotRepoMock.Setup(x => x.GetSessionAsync(sessionId)).ReturnsAsync(session);

        // Act
        var result = await _sut.DeleteSessionAsync(userId, sessionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _chatbotRepoMock.Verify(x => x.DeactivateSessionAsync(sessionId), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
