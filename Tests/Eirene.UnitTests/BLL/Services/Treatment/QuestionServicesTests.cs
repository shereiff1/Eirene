using AutoMapper;
using Eirene.BLL.ModelVMs.Treatment;
using Eirene.BLL.Services.Implementation.Treatment;
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Treatment;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Treatment;

public class QuestionServicesTests
{
    private readonly Mock<IQuestionRepository> _questionRepoMock;
    private readonly Mock<IQuestionChoiceRepository> _questionChoiceRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<QuestionServices>> _loggerMock;
    private readonly QuestionServices _sut;

    public QuestionServicesTests()
    {
        _questionRepoMock = new Mock<IQuestionRepository>();
        _questionChoiceRepoMock = new Mock<IQuestionChoiceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<QuestionServices>>();

        _sut = new QuestionServices(
            _loggerMock.Object,
            _mapperMock.Object,
            _questionRepoMock.Object,
            _questionChoiceRepoMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsQuestion()
    {
        // Arrange
        var model = new AddQuestion { QuestionContent = "What is your mood?" };
        var entity = new Question { QuestionContent = "What is your mood?" };
        var dto = new QuestionDTO { QuestionContent = "What is your mood?" };

        _mapperMock.Setup(x => x.Map<Question>(model)).Returns(entity);
        _questionRepoMock.Setup(x => x.AddAsync(entity)).ReturnsAsync(entity);
        _mapperMock.Setup(x => x.Map<QuestionDTO>(entity)).Returns(dto);

        // Act
        var result = await _sut.CreateAsync(model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Assert.Equivalent(dto, result.AddedQuestion);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_QuestionsExist_ReturnsList()
    {
        // Arrange
        var entities = new List<Question> { new Question { QuestionContent = "Q1" } };
        var dtos = new List<QuestionDTO> { new QuestionDTO { QuestionContent = "Q1" } };

        _questionRepoMock.Setup(x => x.GetAllWithChoicesAsync()).ReturnsAsync(entities);
        _mapperMock.Setup(x => x.Map<List<QuestionDTO>>(entities)).Returns(dtos);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.questions.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteAsync_QuestionExists_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Question { Id = id };
        _questionRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _questionRepoMock.Verify(x => x.DeleteAsync(entity), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsEditedQuestion()
    {
        // Arrange
        var model = new EditQuestion { Id = Guid.NewGuid(), QuestionContent = "Updated" };
        var entity = new Question { Id = model.Id, QuestionContent = "Old" };

        _questionRepoMock.Setup(x => x.GetByIdWithChoicesAsync(model.Id)).ReturnsAsync(entity);
        _questionRepoMock.Setup(x => x.UpdateAsync(entity)).ReturnsAsync(true);
        _mapperMock.Setup(x => x.Map<EditQuestion>(entity)).Returns(model);

        // Act
        var result = await _sut.UpdateAsync(model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
