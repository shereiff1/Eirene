using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Implementation.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Enumerators;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Core;

public class SupervisionServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ISupervisionRequestRepository> _requestRepoMock;
    private readonly Mock<IPatientProfileRepository> _patientRepoMock;
    private readonly Mock<IDoctorProfileRepository> _doctorRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<SupervisionService>> _loggerMock;
    private readonly SupervisionService _sut;

    public SupervisionServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _requestRepoMock = new Mock<ISupervisionRequestRepository>();
        _patientRepoMock = new Mock<IPatientProfileRepository>();
        _doctorRepoMock = new Mock<IDoctorProfileRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailSenderMock = new Mock<IEmailSender>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<SupervisionService>>();

        _sut = new SupervisionService(
            _loggerMock.Object,
            _requestRepoMock.Object,
            _patientRepoMock.Object,
            _doctorRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailSenderMock.Object,
            _backgroundJobServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task RespondToSupervisionRequestAsync_ValidAccept_ReturnsSuccess()
    {
        // Arrange
        var doctorId = "doc-1";
        var patientId = "pat-1";
        var requestId = "req-1";

        var request = _fixture.Build<SupervisionRequest>()
            .With(r => r.DoctorProfileId, doctorId)
            .With(r => r.PatientProfileId, patientId)
            .With(r => r.Status, SupervisionRequestStatus.Pending)
            .Create();

        var patient = _fixture.Build<PatientProfile>().With(p => p.Id, patientId).Create();
        var doctor = _fixture.Build<DoctorProfile>().With(d => d.Id, doctorId).Create();

        var otherRequests = new List<SupervisionRequest> { _fixture.Create<SupervisionRequest>() };

        _requestRepoMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(request);
        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId)).ReturnsAsync(patient);
        _doctorRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _requestRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<SupervisionRequest, bool>>>())).ReturnsAsync(otherRequests);

        // Act
        var result = await _sut.RespondToSupervisionRequestAsync(requestId, accept: true, doctorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(SupervisionRequestStatus.Accepted);
        patient.DoctorProfileId.Should().Be(doctorId);
        _patientRepoMock.Verify(x => x.UpdateAsync(patient), Times.Once);
        _requestRepoMock.Verify(x => x.DeleteRange(otherRequests), Times.Once);
        _backgroundJobServiceMock.Verify(x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RespondToSupervisionRequestAsync_ValidDecline_ReturnsSuccess()
    {
        // Arrange
        var doctorId = "doc-1";
        var requestId = "req-1";

        var request = _fixture.Build<SupervisionRequest>()
            .With(r => r.DoctorProfileId, doctorId)
            .With(r => r.Status, SupervisionRequestStatus.Pending)
            .Create();

        _requestRepoMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(request);

        // Act
        var result = await _sut.RespondToSupervisionRequestAsync(requestId, accept: false, doctorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(SupervisionRequestStatus.Declined);
        _patientRepoMock.Verify(x => x.UpdateAsync(It.IsAny<PatientProfile>()), Times.Never);
        _backgroundJobServiceMock.Verify(x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RespondToSupervisionRequestAsync_UnauthorizedDoctor_ReturnsFailure()
    {
        // Arrange
        var request = _fixture.Build<SupervisionRequest>()
            .With(r => r.DoctorProfileId, "other-doc")
            .Create();

        _requestRepoMock.Setup(x => x.GetByIdAsync("req-1")).ReturnsAsync(request);

        // Act
        var result = await _sut.RespondToSupervisionRequestAsync("req-1", accept: true, "my-doc");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("authorized");
    }

    [Fact]
    public async Task RespondToSupervisionRequestAsync_NotPending_ReturnsFailure()
    {
        // Arrange
        var doctorId = "doc-1";
        var request = _fixture.Build<SupervisionRequest>()
            .With(r => r.DoctorProfileId, doctorId)
            .With(r => r.Status, SupervisionRequestStatus.Accepted)
            .Create();

        _requestRepoMock.Setup(x => x.GetByIdAsync("req-1")).ReturnsAsync(request);

        // Act
        var result = await _sut.RespondToSupervisionRequestAsync("req-1", accept: true, doctorId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already been responded");
    }

    [Fact]
    public async Task RespondToSupervisionRequestAsync_PatientNotFound_ReturnsFailure()
    {
        // Arrange
        var doctorId = "doc-1";
        var patientId = "pat-1";
        var request = _fixture.Build<SupervisionRequest>()
            .With(r => r.DoctorProfileId, doctorId)
            .With(r => r.PatientProfileId, patientId)
            .With(r => r.Status, SupervisionRequestStatus.Pending)
            .Create();

        _requestRepoMock.Setup(x => x.GetByIdAsync("req-1")).ReturnsAsync(request);
        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId)).ReturnsAsync((PatientProfile)null!);

        // Act
        var result = await _sut.RespondToSupervisionRequestAsync("req-1", true, doctorId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Patient profile not found.");
    }

    [Fact]
    public async Task RespondToSupervisionRequestAsync_UnexpectedException_ReturnsFailure()
    {
        // Arrange
        _requestRepoMock.Setup(x => x.GetByIdAsync("req-1")).ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.RespondToSupervisionRequestAsync("req-1", true, "doc-1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("An error occurred while responding to the request.");
    }

    [Fact]
    public async Task RemoveSupervisionOnPatient_ValidPatient_RemovesSupervision()
    {
        // Arrange
        var patientId = "pat-1";
        var doctorId = "doc-1";

        var patient = _fixture.Build<PatientProfile>()
            .With(p => p.Id, patientId)
            .With(p => p.DoctorProfileId, doctorId)
            .Create();
        var doctor = _fixture.Build<DoctorProfile>().With(d => d.Id, doctorId).Create();
        var existingRequest = _fixture.Build<SupervisionRequest>().With(r => r.Status, SupervisionRequestStatus.Accepted).Create();

        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId)).ReturnsAsync(patient);
        _doctorRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _requestRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<SupervisionRequest, bool>>>()))
            .ReturnsAsync(new List<SupervisionRequest> { existingRequest });

        // Act
        var result = await _sut.RemoveSupervisionOnPatient(patientId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        patient.DoctorProfileId.Should().BeNull();
        _patientRepoMock.Verify(x => x.UpdateAsync(patient), Times.Once);
        _requestRepoMock.Verify(x => x.DeleteAsync(existingRequest), Times.Once);
        _backgroundJobServiceMock.Verify(x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveSupervisionOnPatient_NotSupervised_ReturnsSuccess()
    {
        // Arrange
        var patientId = "pat-1";
        var patient = _fixture.Build<PatientProfile>()
            .With(p => p.Id, patientId)
            .With(p => p.DoctorProfileId, (string?)null)
            .Create();

        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId)).ReturnsAsync(patient);

        // Act
        var result = await _sut.RemoveSupervisionOnPatient(patientId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _patientRepoMock.Verify(x => x.UpdateAsync(It.IsAny<PatientProfile>()), Times.Never);
    }
}
