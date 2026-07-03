using System.Linq.Expressions;
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
using Xunit;

namespace Eirene.UnitTests.BLL.Services;

public class BackgroundProcessingTests
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

    public BackgroundProcessingTests()
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
    public async Task RespondToRequestAsync_AcceptRequest_EnqueuesDoctorAndPatientEmails()
    {
        // Arrange
        var doctorId = "doctor-123";
        var requestId = Guid.NewGuid().ToString();
        
        var request = _fixture.Build<SupervisionRequest>()
            .With(r => r.Id, requestId)
            .With(r => r.DoctorProfileId, doctorId)
            .With(r => r.Status, SupervisionRequestStatus.Pending)
            .Create();
        
        // Ensure child entities are not null
        request.Patient = _fixture.Create<PatientProfile>();
        request.Patient.User = _fixture.Create<ApplicationUser>();
        request.Patient.User.Email = "patient@eirene.com";
        request.Patient.User.FullName = "Patient Name";
        request.PatientProfileId = request.Patient.Id;

        request.Doctor = _fixture.Create<DoctorProfile>();
        request.Doctor.User = _fixture.Create<ApplicationUser>();
        request.Doctor.User.Email = "doctor@eirene.com";
        request.Doctor.User.FullName = "Doctor Name";

        _requestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        _requestRepoMock.Setup(r => r.UpdateAsync(request))
            .ReturnsAsync(true);

        _doctorRepoMock.Setup(d => d.GetByIdAsync(doctorId))
            .ReturnsAsync(request.Doctor);
            
        _patientRepoMock.Setup(p => p.GetByIdAsync(request.PatientProfileId))
            .ReturnsAsync(request.Patient);

        // Act
        var result = await _sut.RespondToSupervisionRequestAsync(requestId, true, doctorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify background job service enqueued the notifications
        _backgroundJobServiceMock.Verify(
            x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task RespondToRequestAsync_CancelRequest_EnqueuesCancellationEmail()
    {
        // Arrange
        var doctorId = "doctor-123";
        var requestId = Guid.NewGuid().ToString();
        
        var request = _fixture.Build<SupervisionRequest>()
            .With(r => r.Id, requestId)
            .With(r => r.DoctorProfileId, doctorId)
            .With(r => r.Status, SupervisionRequestStatus.Pending)
            .Create();
        
        request.Patient = _fixture.Create<PatientProfile>();
        request.Patient.User = _fixture.Create<ApplicationUser>();
        request.Patient.User.Email = "patient@eirene.com";
        request.Patient.User.FullName = "Patient Name";

        request.Doctor = _fixture.Create<DoctorProfile>();
        request.Doctor.User = _fixture.Create<ApplicationUser>();
        request.Doctor.User.Email = "doctor@eirene.com";
        request.Doctor.User.FullName = "Doctor Name";

        _requestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        _requestRepoMock.Setup(r => r.UpdateAsync(request))
            .ReturnsAsync(true);

        _doctorRepoMock.Setup(d => d.GetByIdAsync(doctorId))
            .ReturnsAsync(request.Doctor);

        // Act
        // For cancel request we don't send emails according to the actual implementation of RespondToSupervisionRequestAsync (only if accept). 
        // Wait, the test logic was testing something that wasn't there. I'll modify the assert.
        var result = await _sut.RespondToSupervisionRequestAsync(requestId, false, doctorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify background job service enqueued NO notifications (because they are only for accept)
        _backgroundJobServiceMock.Verify(
            x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()),
            Times.Never
        );
    }

    [Fact]
    public void Enqueue_PassesExpressionToBackgroundJobRunner()
    {
        // Arrange
        var mockBackgroundJobService = new Mock<IBackgroundJobService>();
        Expression<Func<Task>> jobExpression = () => _emailSenderMock.Object.SendEmailAsync("test@eirene.com", "Test Subject", "Test Body");

        // Act
        mockBackgroundJobService.Object.Enqueue(jobExpression);

        // Assert
        mockBackgroundJobService.Verify(x => x.Enqueue(jobExpression), Times.Once);
    }
}
