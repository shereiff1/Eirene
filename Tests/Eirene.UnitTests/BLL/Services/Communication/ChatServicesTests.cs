using Eirene.BLL.Services.Implementation.Communication;
using Eirene.DAL.Entities.Communication;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Communication;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Communication;

public class ChatServicesTests
{
    private readonly Mock<IChatRepository> _chatRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ChatServices _sut;

    public ChatServicesTests()
    {
        _chatRepoMock = new Mock<IChatRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ChatServices(_chatRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateConversationAsync_ValidRequest_ReturnsConversation()
    {
        // Arrange
        var doctorId = "doc-1";
        var patientId = "pat-1";
        var expectedConversation = new Conversation { DoctorId = doctorId, PatientId = patientId };
        _chatRepoMock.Setup(x => x.CreateConversationAsync(doctorId, patientId)).ReturnsAsync(expectedConversation);

        // Act
        var result = await _sut.CreateConversationAsync(doctorId, patientId);

        // Assert
        Assert.Equivalent(expectedConversation, result);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("", "pat-1")]
    [InlineData("doc-1", "")]
    [InlineData(null, "pat-1")]
    [InlineData("doc-1", null)]
    public async Task CreateConversationAsync_InvalidIds_ThrowsArgumentException(string doctorId, string patientId)
    {
        // Act
        Func<Task> act = async () => await _sut.CreateConversationAsync(doctorId, patientId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateConversationAsync_SameUser_ThrowsInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _sut.CreateConversationAsync("user-1", "user-1");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveMessageAsync_ValidRequest_ReturnsMessage()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var senderId = "user-1";
        var content = "Hello";
        var conversation = new Conversation { Id = conversationId, PatientId = senderId, DoctorId = "doc-1" };
        var expectedMessage = new ChatMessage { ConversationId = conversationId, SenderId = senderId, Message = content };

        _chatRepoMock.Setup(x => x.GetConversationAsync(conversationId)).ReturnsAsync(conversation);
        _chatRepoMock.Setup(x => x.SaveMessageAsync(conversationId, senderId, content)).ReturnsAsync(expectedMessage);

        // Act
        var result = await _sut.SaveMessageAsync(conversationId, senderId, content);

        // Assert
        Assert.Equivalent(expectedMessage, result);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SaveMessageAsync_ConversationNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        _chatRepoMock.Setup(x => x.GetConversationAsync(conversationId)).ReturnsAsync((Conversation)null!);

        // Act
        Func<Task> act = async () => await _sut.SaveMessageAsync(conversationId, "user-1", "msg");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveMessageAsync_UnauthorizedSender_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var senderId = "intruder";
        var conversation = new Conversation { Id = conversationId, PatientId = "pat-1", DoctorId = "doc-1" };

        _chatRepoMock.Setup(x => x.GetConversationAsync(conversationId)).ReturnsAsync(conversation);

        // Act
        Func<Task> act = async () => await _sut.SaveMessageAsync(conversationId, senderId, "msg");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData(null)]
    public async Task SaveMessageAsync_EmptyMessage_ThrowsArgumentException(string message)
    {
        // Act
        Func<Task> act = async () => await _sut.SaveMessageAsync(Guid.NewGuid(), "user-1", message);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
