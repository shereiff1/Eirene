using AutoFixture;
using AutoMapper;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Admin.Verification;
using Eirene.BLL.Models.Core.Doctor.Verification;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.BLL.Services.Implementation.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Enumerators;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;
using AutoFixture.AutoMoq;

namespace Eirene.UnitTests.BLL.Services.Core;

public class DoctorVerificationServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IDoctorVerificationRepository> _verificationRepoMock;
    private readonly Mock<IDoctorDocumentRepository> _documentRepoMock;
    private readonly Mock<IDoctorAuditLogRepository> _auditLogRepoMock;
    private readonly Mock<IDoctorProfileRepository> _doctorProfileRepoMock;
    private readonly Mock<IDocumentStorageService> _documentStorageMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<DoctorVerificationService>> _loggerMock;
    private readonly DoctorVerificationService _sut;

    public DoctorVerificationServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _verificationRepoMock = new Mock<IDoctorVerificationRepository>();
        _documentRepoMock = new Mock<IDoctorDocumentRepository>();
        _auditLogRepoMock = new Mock<IDoctorAuditLogRepository>();
        _doctorProfileRepoMock = new Mock<IDoctorProfileRepository>();
        _documentStorageMock = new Mock<IDocumentStorageService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<DoctorVerificationService>>();

        _sut = new DoctorVerificationService(
            _verificationRepoMock.Object,
            _documentRepoMock.Object,
            _auditLogRepoMock.Object,
            _doctorProfileRepoMock.Object,
            _documentStorageMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SubmitDoctorDocumentsAsync_NewVerification_ReturnsSuccess()
    {
        // Arrange
        var doctorId = "doc-123";
        var request = _fixture.Build<SubmitDocumentsRequest>()
            .With(x => x.Files, new List<IFormFile>())
            .With(x => x.DocumentTypes, new List<DocumentType>())
            .Create();
        
        var doctor = _fixture.Create<DoctorProfile>();
        var mappedModel = _fixture.Create<DoctorVerificationModel>();

        _doctorProfileRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _verificationRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorVerification, bool>>>()))
            .ReturnsAsync(new List<DoctorVerification>()); // No existing verification

        _mapperMock.Setup(x => x.Map<DoctorVerificationModel>(It.IsAny<DoctorVerification>())).Returns(mappedModel);
        _documentRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorDocument, bool>>>()))
            .ReturnsAsync(new List<DoctorDocument>());
        _mapperMock.Setup(x => x.Map<List<DoctorDocumentModel>>(It.IsAny<List<DoctorDocument>>()))
            .Returns(new List<DoctorDocumentModel>());

        // Act
        var result = await _sut.SubmitDoctorDocumentsAsync(doctorId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _verificationRepoMock.Verify(x => x.AddAsync(It.Is<DoctorVerification>(v => v.VerificationStatus == VerificationStatus.Pending)), Times.Once);
        _verificationRepoMock.Verify(x => x.UpdateAsync(It.IsAny<DoctorVerification>()), Times.Never);
        _auditLogRepoMock.Verify(x => x.AddAsync(It.Is<DoctorAuditLog>(a => a.Action == "DocumentsSubmitted")), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitDoctorDocumentsAsync_ExistingVerification_UpdatesAndReturnsSuccess()
    {
        // Arrange
        var doctorId = "doc-123";
        var request = _fixture.Build<SubmitDocumentsRequest>()
            .With(x => x.Files, new List<IFormFile>())
            .With(x => x.DocumentTypes, new List<DocumentType>())
            .Create();
        
        var doctor = _fixture.Create<DoctorProfile>();
        var existingVerification = _fixture.Create<DoctorVerification>();
        var mappedModel = _fixture.Create<DoctorVerificationModel>();

        _doctorProfileRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _verificationRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorVerification, bool>>>()))
            .ReturnsAsync(new List<DoctorVerification> { existingVerification });

        _mapperMock.Setup(x => x.Map<DoctorVerificationModel>(It.IsAny<DoctorVerification>())).Returns(mappedModel);
        _documentRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorDocument, bool>>>()))
            .ReturnsAsync(new List<DoctorDocument>());

        // Act
        var result = await _sut.SubmitDoctorDocumentsAsync(doctorId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _verificationRepoMock.Verify(x => x.UpdateAsync(It.Is<DoctorVerification>(v => v.VerificationStatus == VerificationStatus.Pending)), Times.Once);
        _verificationRepoMock.Verify(x => x.AddAsync(It.IsAny<DoctorVerification>()), Times.Never);
    }

    [Fact]
    public async Task SubmitDoctorDocumentsAsync_DoctorNotFound_ReturnsFailure()
    {
        // Arrange
        var doctorId = "invalid-doc";
        var request = _fixture.Create<SubmitDocumentsRequest>();

        _doctorProfileRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync((DoctorProfile?)null);

        // Act
        var result = await _sut.SubmitDoctorDocumentsAsync(doctorId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Doctor profile not found");
        _verificationRepoMock.Verify(x => x.AddAsync(It.IsAny<DoctorVerification>()), Times.Never);
    }

    [Fact]
    public async Task SubmitDoctorDocumentsAsync_UploadFails_ReturnsFailure()
    {
        // Arrange
        var doctorId = "doc-123";
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.pdf");

        var request = new SubmitDocumentsRequest 
        { 
            Files = new List<IFormFile> { fileMock.Object },
            DocumentTypes = new List<DocumentType> { DocumentType.NationalId }
        };
        
        var doctor = _fixture.Create<DoctorProfile>();
        _doctorProfileRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _verificationRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorVerification, bool>>>()))
            .ReturnsAsync(new List<DoctorVerification>());

        _documentStorageMock.Setup(x => x.UploadDocumentAsync(It.IsAny<IFormFile>(), doctorId))
            .ReturnsAsync((false, null!, "Upload error"));

        // Act
        var result = await _sut.SubmitDoctorDocumentsAsync(doctorId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to upload document");
        _auditLogRepoMock.Verify(x => x.AddAsync(It.IsAny<DoctorAuditLog>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(VerificationStatus.Pending, VerificationStatus.UnderReview, true)]
    [InlineData(VerificationStatus.UnderReview, VerificationStatus.Approved, true)]
    [InlineData(VerificationStatus.UnderReview, VerificationStatus.Rejected, true)]
    [InlineData(VerificationStatus.Approved, VerificationStatus.Suspended, true)]
    [InlineData(VerificationStatus.Suspended, VerificationStatus.Approved, true)]
    [InlineData(VerificationStatus.Pending, VerificationStatus.Approved, false)] // Invalid transition
    [InlineData(VerificationStatus.Approved, VerificationStatus.Pending, false)] // Invalid transition
    public async Task ReviewDoctorAsync_StateTransitions_ValidatesCorrectly(VerificationStatus currentStatus, VerificationStatus newStatus, bool isValid)
    {
        // Arrange
        var adminId = "admin-1";
        var doctorId = "doc-1";
        var request = new ReviewDoctorRequest { NewStatus = newStatus, Reason = "Test" };
        
        var verification = _fixture.Build<DoctorVerification>()
            .With(v => v.VerificationStatus, currentStatus)
            .Create();
        var doctor = _fixture.Create<DoctorProfile>();

        _verificationRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorVerification, bool>>>()))
            .ReturnsAsync(new List<DoctorVerification> { verification });
        _doctorProfileRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        
        _mapperMock.Setup(x => x.Map<DoctorVerificationModel>(It.IsAny<DoctorVerification>()))
            .Returns(new DoctorVerificationModel());
        _documentRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorDocument, bool>>>()))
            .ReturnsAsync(new List<DoctorDocument>());

        // Act
        var result = await _sut.ReviewDoctorAsync(adminId, doctorId, request);

        // Assert
        if (isValid)
        {
            result.IsSuccess.Should().BeTrue();
            _verificationRepoMock.Verify(x => x.UpdateAsync(It.Is<DoctorVerification>(v => v.VerificationStatus == newStatus)), Times.Once);
            _auditLogRepoMock.Verify(x => x.AddAsync(It.Is<DoctorAuditLog>(a => a.Action == $"StatusChangedTo{newStatus}")), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            if (newStatus == VerificationStatus.Approved)
            {
                _doctorProfileRepoMock.Verify(x => x.UpdateAsync(It.Is<DoctorProfile>(d => d.IsVerified == true)), Times.Once);
            }
            else if (newStatus == VerificationStatus.Rejected || newStatus == VerificationStatus.Suspended)
            {
                _doctorProfileRepoMock.Verify(x => x.UpdateAsync(It.Is<DoctorProfile>(d => d.IsVerified == false)), Times.Once);
            }
        }
        else
        {
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Invalid state transition");
            _verificationRepoMock.Verify(x => x.UpdateAsync(It.IsAny<DoctorVerification>()), Times.Never);
        }
    }

    [Fact]
    public async Task ReviewDoctorAsync_VerificationNotFound_ReturnsFailure()
    {
        // Arrange
        _verificationRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorVerification, bool>>>()))
            .ReturnsAsync(new List<DoctorVerification>());

        // Act
        var result = await _sut.ReviewDoctorAsync("admin", "doc", new ReviewDoctorRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("record not found");
    }

    [Fact]
    public async Task GetPendingDoctorsAsync_ReturnsPendingAndUnderReview()
    {
        // Arrange
        var verifications = new List<DoctorVerification>
        {
            _fixture.Build<DoctorVerification>().With(v => v.VerificationStatus, VerificationStatus.Pending).Create(),
            _fixture.Build<DoctorVerification>().With(v => v.VerificationStatus, VerificationStatus.UnderReview).Create()
        };

        _verificationRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<DoctorVerification, bool>>>()))
            .ReturnsAsync(verifications);
        
        _mapperMock.Setup(x => x.Map<DoctorVerificationModel>(It.IsAny<DoctorVerification>()))
            .Returns(new DoctorVerificationModel());

        // Act
        var result = await _sut.GetPendingDoctorsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
