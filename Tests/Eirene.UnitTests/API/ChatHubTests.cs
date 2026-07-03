using System.Security.Claims;
using Eirene.BLL.Hubs;
using Eirene.BLL.Services.Abstraction.Communication;
using Eirene.DAL.Entities.Communication;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Eirene.UnitTests.API;

public class ChatHubTests
{
    private readonly Mock<IChatServices> _chatServicesMock;
    private readonly Mock<IGroupManager> _groupsMock;
    private readonly Mock<IHubCallerClients> _clientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly ChatHub _sut;

    public ChatHubTests()
    {
        _chatServicesMock = new Mock<IChatServices>();
        _groupsMock = new Mock<IGroupManager>();
        _clientsMock = new Mock<IHubCallerClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        _contextMock = new Mock<HubCallerContext>();

        // Set up the Hub with mocked clients and context
        _sut = new ChatHub(_chatServicesMock.Object)
        {
            Context = _contextMock.Object,
            Groups = _groupsMock.Object,
            Clients = _clientsMock.Object
        };
    }

    private void SetupUser(string? userId)
    {
        if (userId == null)
        {
            _contextMock.Setup(c => c.User).Returns((ClaimsPrincipal)null!);
        }
        else
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _contextMock.Setup(c => c.User).Returns(principal);
        }
        _contextMock.Setup(c => c.ConnectionId).Returns("conn-123");
    }

    [Fact]
    public async Task JoinConversation_UnauthorizedUser_ThrowsHubException()
    {
        // Arrange
        SetupUser(null);
        var conversationId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _sut.JoinConversation(conversationId);

        // Assert
        await act.Should().ThrowAsync<HubException>().WithMessage("*Unauthorized*");
    }

    [Fact]
    public async Task JoinConversation_ConversationNotFound_ThrowsHubException()
    {
        // Arrange
        SetupUser("user-1");
        var conversationId = Guid.NewGuid();
        _chatServicesMock.Setup(x => x.GetConversationAsync(conversationId))
            .ReturnsAsync((Conversation?)null);

        // Act
        Func<Task> act = async () => await _sut.JoinConversation(conversationId);

        // Assert
        await act.Should().ThrowAsync<HubException>().WithMessage("Conversation not found.");
    }

    [Fact]
    public async Task JoinConversation_NotParticipant_ThrowsHubException()
    {
        // Arrange
        SetupUser("unrelated-user");
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            DoctorId = "doctor-123",
            PatientId = "patient-123"
        };
        _chatServicesMock.Setup(x => x.GetConversationAsync(conversationId))
            .ReturnsAsync(conversation);

        // Act
        Func<Task> act = async () => await _sut.JoinConversation(conversationId);

        // Assert
        await act.Should().ThrowAsync<HubException>().WithMessage("You are not a participant in this conversation.");
    }

    [Fact]
    public async Task JoinConversation_ValidParticipant_SuccessfullyAddsToGroup()
    {
        // Arrange
        SetupUser("patient-123");
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            DoctorId = "doctor-123",
            PatientId = "patient-123"
        };
        _chatServicesMock.Setup(x => x.GetConversationAsync(conversationId))
            .ReturnsAsync(conversation);
        _groupsMock.Setup(g => g.AddToGroupAsync("conn-123", conversationId.ToString(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.JoinConversation(conversationId);

        // Assert
        _groupsMock.Verify(g => g.AddToGroupAsync("conn-123", conversationId.ToString(), default), Times.Once);
    }

    [Fact]
    public async Task SendMessage_ValidUser_SavesMessageAndBroadcastsToGroup()
    {
        // Arrange
        SetupUser("doctor-123");
        var conversationId = Guid.NewGuid();
        var messageText = "Hello Patient!";
        var savedMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = "doctor-123",
            Message = messageText
        };

        _chatServicesMock.Setup(x => x.SaveMessageAsync(conversationId, "doctor-123", messageText))
            .ReturnsAsync(savedMessage);
        
        _clientsMock.Setup(c => c.Group(conversationId.ToString()))
            .Returns(_clientProxyMock.Object);

        // Act
        await _sut.SendMessage(conversationId, messageText);

        // Assert
        _chatServicesMock.Verify(x => x.SaveMessageAsync(conversationId, "doctor-123", messageText), Times.Once);
        _clientProxyMock.Verify(c => c.SendCoreAsync("ReceiveMessage", It.Is<object[]>(o => o[0] == savedMessage), default), Times.Once);
    }
}
