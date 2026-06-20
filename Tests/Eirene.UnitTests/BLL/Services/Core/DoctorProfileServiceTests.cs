using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Implementation.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Core;

public class DoctorProfileServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IDoctorProfileRepository> _doctorRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<HybridCache> _cacheMock;
    private readonly Mock<ILogger<DoctorProfileService>> _loggerMock;
    private readonly DoctorProfileService _sut;

    public DoctorProfileServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _doctorRepoMock = new Mock<IDoctorProfileRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<HybridCache>();
        _loggerMock = new Mock<ILogger<DoctorProfileService>>();

        _sut = new DoctorProfileService(
            _loggerMock.Object,
            _mapperMock.Object,
            _doctorRepoMock.Object,
            _cacheMock.Object,
            _unitOfWorkMock.Object);
    }

    // ========== GetAllAsync ==========

    [Fact]
    public async Task GetAllAsync_DoctorsExist_ReturnsSuccess()
    {
        // Arrange
        var doctors = new List<DoctorProfile>
        {
            new DoctorProfile { Id = "doc-1" },
            new DoctorProfile { Id = "doc-2" }
        };
        var doctorModels = new List<DoctorModel>
        {
            new DoctorModel(),
            new DoctorModel()
        };

        _doctorRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(doctors);
        _mapperMock.Setup(x => x.Map<List<DoctorModel>>(doctors)).Returns(doctorModels);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_NoDoctors_ReturnsFailure()
    {
        // Arrange
        _doctorRepoMock.Setup(x => x.GetAllAsync())!.ReturnsAsync((List<DoctorProfile>?)null);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No doctors found");
    }

    // ========== CreateDoctorProfileAsync ==========

    [Fact]
    public async Task CreateDoctorProfileAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-1";
        var model = _fixture.Create<AddDoctorProfile>();
        var doctorEntity = new DoctorProfile { Id = userId };
        var createdDoctor = new DoctorProfile { Id = userId };
        var doctorModel = new DoctorModel();

        _doctorRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<DoctorProfile>());
        _mapperMock.Setup(x => x.Map<DoctorProfile>(model)).Returns(doctorEntity);
        _doctorRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(createdDoctor);
        _mapperMock.Setup(x => x.Map<DoctorModel>(createdDoctor)).Returns(doctorModel);

        // Act
        var result = await _sut.CreateDoctorProfileAsync(model, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _doctorRepoMock.Verify(x => x.AddAsync(It.Is<DoctorProfile>(d => d.Id == userId)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateDoctorProfileAsync_ProfileAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var userId = "user-1";
        var model = _fixture.Create<AddDoctorProfile>();
        var existingProfile = new DoctorProfile { Id = userId };

        _doctorRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<DoctorProfile> { existingProfile });

        // Act
        var result = await _sut.CreateDoctorProfileAsync(model, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
        _doctorRepoMock.Verify(x => x.AddAsync(It.IsAny<DoctorProfile>()), Times.Never);
    }

    // ========== UpdateDoctorProfileAsync ==========

    [Fact]
    public async Task UpdateDoctorProfileAsync_ValidRequest_InvalidatesCache()
    {
        // Arrange
        var userId = "user-1";
        var model = _fixture.Create<EditDoctorProfile>();
        var existingProfile = new DoctorProfile { Id = userId };
        var doctorModel = new DoctorModel();

        _doctorRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<DoctorProfile> { existingProfile });
        _mapperMock.Setup(x => x.Map<DoctorModel>(existingProfile)).Returns(doctorModel);

        // Act
        var result = await _sut.UpdateDoctorProfileAsync(model, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cacheMock.Verify(x => x.RemoveAsync($"doctor:{userId}", default), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateDoctorProfileAsync_ProfileNotFound_ReturnsFailure()
    {
        // Arrange
        var model = _fixture.Create<EditDoctorProfile>();
        _doctorRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<DoctorProfile>());

        // Act
        var result = await _sut.UpdateDoctorProfileAsync(model, "no-user");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    // ========== DeleteDoctorProfile ==========

    [Fact]
    public async Task DeleteDoctorProfile_ValidRequest_InvalidatesCache()
    {
        // Arrange
        var doctorId = "doc-1";
        var doctor = new DoctorProfile { Id = doctorId };

        _doctorRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);

        // Act
        var result = await _sut.DeleteDoctorProfile(doctorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _doctorRepoMock.Verify(x => x.DeleteAsync(doctor), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync($"doctor:{doctorId}", default), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteDoctorProfile_NotFound_ReturnsFailure()
    {
        // Arrange
        _doctorRepoMock.Setup(x => x.GetByIdAsync("no-id")).ReturnsAsync((DoctorProfile?)null);

        // Act
        var result = await _sut.DeleteDoctorProfile("no-id");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    // ========== CheckIfVerified ==========

    [Fact]
    public async Task CheckIfVerified_VerifiedDoctor_ReturnsTrue()
    {
        // Arrange
        var doctor = new DoctorProfile { Id = "doc-1", IsVerified = true };
        _doctorRepoMock.Setup(x => x.GetByIdAsync("doc-1")).ReturnsAsync(doctor);

        // Act
        var result = await _sut.CheckIfVerified("doc-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task CheckIfVerified_UnverifiedDoctor_ReturnsFalse()
    {
        // Arrange
        var doctor = new DoctorProfile { Id = "doc-1", IsVerified = false };
        _doctorRepoMock.Setup(x => x.GetByIdAsync("doc-1")).ReturnsAsync(doctor);

        // Act
        var result = await _sut.CheckIfVerified("doc-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task CheckIfVerified_DoctorNotFound_ReturnsFailure()
    {
        // Arrange
        _doctorRepoMock.Setup(x => x.GetByIdAsync("no-id")).ReturnsAsync((DoctorProfile?)null);

        // Act
        var result = await _sut.CheckIfVerified("no-id");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Not Found");
    }
}
