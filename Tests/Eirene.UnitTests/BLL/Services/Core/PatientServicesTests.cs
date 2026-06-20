using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Models.Core.Patient;
using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Implementation.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Enumerators;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Community;
using Eirene.DAL.Repository.Abstraction.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Core;

public class PatientServicesTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPatientProfileRepository> _patientRepoMock;
    private readonly Mock<IDoctorProfileRepository> _doctorRepoMock;
    private readonly Mock<ISupervisionRequestRepository> _requestRepoMock;
    private readonly Mock<IDoctorRatingRepository> _ratingRepoMock;
    private readonly Mock<IApplicationUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobMock;
    private readonly Mock<IUserCommunityGroupRepository> _userCommunityGroupRepoMock;
    private readonly Mock<ILogger<PatientServices>> _loggerMock;
    private readonly PatientServices _sut;

    public PatientServicesTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _patientRepoMock = new Mock<IPatientProfileRepository>();
        _doctorRepoMock = new Mock<IDoctorProfileRepository>();
        _requestRepoMock = new Mock<ISupervisionRequestRepository>();
        _ratingRepoMock = new Mock<IDoctorRatingRepository>();
        _userRepoMock = new Mock<IApplicationUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _emailSenderMock = new Mock<IEmailSender>();
        _backgroundJobMock = new Mock<IBackgroundJobService>();
        _userCommunityGroupRepoMock = new Mock<IUserCommunityGroupRepository>();
        _loggerMock = new Mock<ILogger<PatientServices>>();

        _sut = new PatientServices(
            _loggerMock.Object,
            _mapperMock.Object,
            _patientRepoMock.Object,
            _doctorRepoMock.Object,
            _requestRepoMock.Object,
            _ratingRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailSenderMock.Object,
            _backgroundJobMock.Object,
            _userCommunityGroupRepoMock.Object);
    }

    // ========== RequestSupervisionAsync ==========

    [Fact]
    public async Task RequestSupervisionAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var patientId = "pat-1";
        var doctorId = "doc-1";

        var patient = new PatientProfile
        {
            Id = patientId,
            DoctorProfileId = null,
            User = new ApplicationUser { Email = "patient@test.com" }
        };
        var doctor = new DoctorProfile
        {
            Id = doctorId,
            User = new ApplicationUser { Email = "doc@test.com", FullName = "Dr. Smith" }
        };

        _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PatientProfile> { patient });
        _doctorRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _requestRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<SupervisionRequest, bool>>>()))
            .ReturnsAsync(new List<SupervisionRequest>());

        // Act
        var (isSuccess, error) = await _sut.RequestSupervisionAsync(patientId, doctorId);

        // Assert
        isSuccess.Should().BeTrue();
        error.Should().BeNull();
        _requestRepoMock.Verify(x => x.AddAsync(It.Is<SupervisionRequest>(
            r => r.PatientProfileId == patientId && r.DoctorProfileId == doctorId && r.Status == SupervisionRequestStatus.Pending)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _backgroundJobMock.Verify(x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RequestSupervisionAsync_PatientNotFound_ReturnsFailure()
    {
        // Arrange
        _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PatientProfile>());

        // Act
        var (isSuccess, error) = await _sut.RequestSupervisionAsync("no-patient", "doc-1");

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("Patient profile not found");
    }

    [Fact]
    public async Task RequestSupervisionAsync_AlreadySupervised_ReturnsFailure()
    {
        // Arrange
        var patient = new PatientProfile { Id = "pat-1", DoctorProfileId = "existing-doc" };
        _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PatientProfile> { patient });

        // Act
        var (isSuccess, error) = await _sut.RequestSupervisionAsync("pat-1", "doc-1");

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("already under a doctor's supervision");
    }

    [Fact]
    public async Task RequestSupervisionAsync_DoctorNotFound_ReturnsFailure()
    {
        // Arrange
        var patient = new PatientProfile { Id = "pat-1", DoctorProfileId = null };
        _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PatientProfile> { patient });
        _doctorRepoMock.Setup(x => x.GetByIdAsync("doc-1")).ReturnsAsync((DoctorProfile?)null);

        // Act
        var (isSuccess, error) = await _sut.RequestSupervisionAsync("pat-1", "doc-1");

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("Doctor not found");
    }

    [Fact]
    public async Task RequestSupervisionAsync_PendingRequestExists_ReturnsFailure()
    {
        // Arrange
        var patient = new PatientProfile { Id = "pat-1", DoctorProfileId = null };
        var doctor = new DoctorProfile { Id = "doc-1" };
        var existingRequest = new SupervisionRequest { Status = SupervisionRequestStatus.Pending };

        _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PatientProfile> { patient });
        _doctorRepoMock.Setup(x => x.GetByIdAsync("doc-1")).ReturnsAsync(doctor);
        _requestRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<SupervisionRequest, bool>>>()))
            .ReturnsAsync(new List<SupervisionRequest> { existingRequest });

        // Act
        var (isSuccess, error) = await _sut.RequestSupervisionAsync("pat-1", "doc-1");

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("pending request");
    }

    // ========== CreatePatientProfileAsync ==========

    [Fact]
    public async Task CreatePatientProfileAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-1";
        var model = _fixture.Create<AddPatientProfile>();
        var user = new ApplicationUser { Id = userId, Email = "test@mail.com" };
        var patientEntity = new PatientProfile { Id = userId };
        var patientModel = _fixture.Create<PatientModel>();

        _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PatientProfile>());
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<PatientProfile>(model)).Returns(patientEntity);
        _mapperMock.Setup(x => x.Map<PatientModel>(patientEntity)).Returns(patientModel);

        // Act
        var (isSuccess, error, patient) = await _sut.CreatePatientProfileAsync(model, userId);

        // Assert
        isSuccess.Should().BeTrue();
        error.Should().BeNull();
        Assert.NotNull(patient);
        _patientRepoMock.Verify(x => x.AddAsync(It.Is<PatientProfile>(p => p.Id == userId)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreatePatientProfileAsync_ProfileAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var userId = "user-1";
        var model = _fixture.Create<AddPatientProfile>();
        var existingProfile = new PatientProfile { Id = userId };

        _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PatientProfile> { existingProfile });

        // Act
        var (isSuccess, error, patient) = await _sut.CreatePatientProfileAsync(model, userId);

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("already exists");
        Assert.Null(patient);
    }

    [Fact]
    public async Task CreatePatientProfileAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = "user-1";
        var model = _fixture.Create<AddPatientProfile>();

        _patientRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PatientProfile>());
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

        // Act
        var (isSuccess, error, patient) = await _sut.CreatePatientProfileAsync(model, userId);

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("User account not found");
    }

    // ========== RateSupervisorAsync ==========

    [Fact]
    public async Task RateSupervisorAsync_NewRating_CreatesAndRecalculates()
    {
        // Arrange
        var patientId = "pat-1";
        var doctorId = "doc-1";
        var ratingModel = new AddDoctorRatingDTO { Rating = 4, Review = "Great doctor" };
        var patient = new PatientProfile { Id = patientId, DoctorProfileId = doctorId };
        var doctor = new DoctorProfile { Id = doctorId, Rating = 0, ReviewCount = 0 };

        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId)).ReturnsAsync(patient);
        _doctorRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _ratingRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorRating, bool>>>()))
            .ReturnsAsync(new List<DoctorRating>()); // No existing rating

        var allRatings = new List<DoctorRating> { new DoctorRating { Rating = 4 } };
        _ratingRepoMock.SetupSequence(x => x.FindAsync(It.IsAny<Expression<Func<DoctorRating, bool>>>()))
            .ReturnsAsync(new List<DoctorRating>())   // First call: check existing rating
            .ReturnsAsync(allRatings);                  // Second call: all ratings for average

        // Act
        var (isSuccess, error) = await _sut.RateSupervisorAsync(patientId, doctorId, ratingModel);

        // Assert
        isSuccess.Should().BeTrue();
        error.Should().BeNull();
        _ratingRepoMock.Verify(x => x.AddAsync(It.Is<DoctorRating>(
            r => r.Rating == 4 && r.Review == "Great doctor")), Times.Once);
        doctor.ReviewCount.Should().Be(1);
        doctor.Rating.Should().Be(4);
    }

    [Fact]
    public async Task RateSupervisorAsync_NotAssignedDoctor_ReturnsFailure()
    {
        // Arrange
        var patient = new PatientProfile { Id = "pat-1", DoctorProfileId = "other-doc" };
        _patientRepoMock.Setup(x => x.GetByIdAsync("pat-1")).ReturnsAsync(patient);

        var model = new AddDoctorRatingDTO { Rating = 5, Review = "Test" };

        // Act
        var (isSuccess, error) = await _sut.RateSupervisorAsync("pat-1", "doc-1", model);

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("only rate your assigned supervisor");
    }

    [Fact]
    public async Task RateSupervisorAsync_PatientNotFound_ReturnsFailure()
    {
        // Arrange
        _patientRepoMock.Setup(x => x.GetByIdAsync("no-patient")).ReturnsAsync((PatientProfile?)null);

        // Act
        var (isSuccess, error) = await _sut.RateSupervisorAsync("no-patient", "doc-1", new AddDoctorRatingDTO());

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("Patient profile not found");
    }

    // ========== MarkAsDiagnosedAsync ==========

    [Fact]
    public async Task MarkAsDiagnosedAsync_ValidPatient_SetsIsDiagnosed()
    {
        // Arrange
        var patient = new PatientProfile { Id = "pat-1", IsDiagnosed = false };
        _patientRepoMock.Setup(x => x.GetByIdAsync("pat-1")).ReturnsAsync(patient);

        // Act
        var (isSuccess, error) = await _sut.MarkAsDiagnosedAsync("pat-1");

        // Assert
        isSuccess.Should().BeTrue();
        patient.IsDiagnosed.Should().BeTrue();
        _patientRepoMock.Verify(x => x.UpdateAsync(patient), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkAsDiagnosedAsync_PatientNotFound_ReturnsFailure()
    {
        // Arrange
        _patientRepoMock.Setup(x => x.GetByIdAsync("no-id")).ReturnsAsync((PatientProfile?)null);

        // Act
        var (isSuccess, error) = await _sut.MarkAsDiagnosedAsync("no-id");

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("Patient profile not found");
    }

    // ========== CheckIfDiagnosedAsync ==========

    [Fact]
    public async Task CheckIfDiagnosedAsync_DiagnosedPatient_ReturnsTrue()
    {
        // Arrange
        var patient = new PatientProfile { Id = "pat-1", IsDiagnosed = true };
        _patientRepoMock.Setup(x => x.GetByIdAsync("pat-1")).ReturnsAsync(patient);

        // Act
        var (isSuccess, isDiagnosed, error) = await _sut.CheckIfDiagnosedAsync("pat-1");

        // Assert
        isSuccess.Should().BeTrue();
        isDiagnosed.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task CheckIfDiagnosedAsync_NotDiagnosedPatient_ReturnsFalse()
    {
        // Arrange
        var patient = new PatientProfile { Id = "pat-1", IsDiagnosed = false };
        _patientRepoMock.Setup(x => x.GetByIdAsync("pat-1")).ReturnsAsync(patient);

        // Act
        var (isSuccess, isDiagnosed, error) = await _sut.CheckIfDiagnosedAsync("pat-1");

        // Assert
        isSuccess.Should().BeTrue();
        isDiagnosed.Should().BeFalse();
    }

    // ========== RemoveDoctorSupervision ==========

    [Fact]
    public async Task RemoveDoctorSupervision_ValidRequest_RemovesAndNotifies()
    {
        // Arrange
        var patientId = "pat-1";
        var doctorId = "doc-1";

        var patient = new PatientProfile
        {
            Id = patientId,
            DoctorProfileId = doctorId,
            User = new ApplicationUser { Email = "p@t.com", FullName = "Patient" }
        };
        var doctor = new DoctorProfile
        {
            Id = doctorId,
            User = new ApplicationUser { Email = "d@t.com", FullName = "Doctor" }
        };
        var existingRequest = new SupervisionRequest { Id = "req-1" };

        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId)).ReturnsAsync(patient);
        _doctorRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _requestRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<SupervisionRequest, bool>>>()))
            .ReturnsAsync(new List<SupervisionRequest> { existingRequest });

        // Act
        var (isSuccess, error) = await _sut.RemoveDoctorSupervision(patientId, doctorId);

        // Assert
        isSuccess.Should().BeTrue();
        patient.DoctorProfileId.Should().BeNull();
        _requestRepoMock.Verify(x => x.DeleteAsync(existingRequest), Times.Once);
        _patientRepoMock.Verify(x => x.UpdateAsync(patient), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _backgroundJobMock.Verify(x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RemoveDoctorSupervision_NoExistingRequest_ReturnsFailure()
    {
        // Arrange
        var patient = new PatientProfile { Id = "pat-1" };
        var doctor = new DoctorProfile { Id = "doc-1" };

        _patientRepoMock.Setup(x => x.GetByIdAsync("pat-1")).ReturnsAsync(patient);
        _doctorRepoMock.Setup(x => x.GetByIdAsync("doc-1")).ReturnsAsync(doctor);
        _requestRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<SupervisionRequest, bool>>>()))
            .ReturnsAsync(new List<SupervisionRequest>());

        // Act
        var (isSuccess, error) = await _sut.RemoveDoctorSupervision("pat-1", "doc-1");

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("no supervision request found");
    }

    // ========== DeletePatientProfileAsync ==========

    [Fact]
    public async Task DeletePatientProfileAsync_ValidProfile_ReturnsSuccess()
    {
        // Arrange
        var patient = new PatientProfile { Id = "pat-1" };
        _patientRepoMock.Setup(x => x.GetByIdAsync("pat-1")).ReturnsAsync(patient);

        // Act
        var (isSuccess, error) = await _sut.DeletePatientProfileAsync("pat-1");

        // Assert
        isSuccess.Should().BeTrue();
        _patientRepoMock.Verify(x => x.DeleteAsync(patient), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePatientProfileAsync_NotFound_ReturnsFailure()
    {
        // Arrange
        _patientRepoMock.Setup(x => x.GetByIdAsync("no-id")).ReturnsAsync((PatientProfile?)null);

        // Act
        var (isSuccess, error) = await _sut.DeletePatientProfileAsync("no-id");

        // Assert
        isSuccess.Should().BeFalse();
        error.Should().Contain("Patient profile not found");
    }
}
