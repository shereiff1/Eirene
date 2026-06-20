using Eirene.BLL.Services.Implementation.Treatment;
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Treatment;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Treatment;

public class QuestionAnswerServicesTests
{
    private readonly Mock<IQuestionAnswerRepository> _answerRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<QuestionAnswerServices>> _loggerMock;
    private readonly QuestionAnswerServices _sut;

    public QuestionAnswerServicesTests()
    {
        _answerRepoMock = new Mock<IQuestionAnswerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<QuestionAnswerServices>>();

        _sut = new QuestionAnswerServices(
            _loggerMock.Object,
            _answerRepoMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task GetAnswersForUserAsync_ValidUser_ReturnsAnswers()
    {
        // Arrange
        var userId = "user-1";
        var answers = new List<QuestionAnswer> { new QuestionAnswer { PatientId = userId } };
        _answerRepoMock.Setup(x => x.GetAnswersByUserIdAsync(userId)).ReturnsAsync(answers);

        // Act
        var result = await _sut.GetAnswersForUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Answers.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAnswerAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-1";
        var questionId = Guid.NewGuid();
        var answerText = "I feel good";
        var expectedAnswer = new QuestionAnswer { PatientId = userId, QuestionId = questionId, Answer = answerText };

        _answerRepoMock.Setup(x => x.AddAsync(It.IsAny<QuestionAnswer>())).ReturnsAsync(expectedAnswer);

        // Act
        var result = await _sut.AddAnswerAsync(userId, questionId, answerText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Assert.Equivalent(expectedAnswer, result.Answer);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddMultipleAnswersAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-1";
        var answers = new List<(Guid QuestionId, string Answer)>
        {
            (Guid.NewGuid(), "A1"),
            (Guid.NewGuid(), "A2")
        };

        _answerRepoMock.Setup(x => x.AddAsync(It.IsAny<QuestionAnswer>()))
            .ReturnsAsync((QuestionAnswer qa) => qa);

        // Act
        var result = await _sut.AddMultipleAnswersAsync(userId, answers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Answers.Should().HaveCount(2);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
