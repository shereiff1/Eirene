using AutoMapper;
using Eirene.BLL.Enumerators;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Implementation.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Core;

public class RoleManagementServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IDoctorProfileRepository> _doctorRepoMock;
    private readonly Mock<IPatientProfileRepository> _patientRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RoleManagementService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RoleManagementService _sut;

    public RoleManagementServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _doctorRepoMock = new Mock<IDoctorProfileRepository>();
        _patientRepoMock = new Mock<IPatientProfileRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RoleManagementService>>();
        _mapperMock = new Mock<IMapper>();

        _sut = new RoleManagementService(
            _userManagerMock.Object,
            _doctorRepoMock.Object,
            _patientRepoMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task AssignRoleAsync_ValidRequest_AssignsRoleAndCreatesProfile()
    {
        // Arrange
        var adminId = "admin-1";
        var userId = "user-1";
        var role = Roles.Doctor;
        var user = new ApplicationUser { Id = userId, Email = "test@test.com" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, role)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.AssignRoleAsync(adminId, userId, role);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _doctorRepoMock.Verify(x => x.AddAsync(It.IsAny<DoctorProfile>()), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, role), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_SelfAssignment_ReturnsFailure()
    {
        // Act
        var result = await _sut.AssignRoleAsync("user-1", "user-1", Roles.Admin);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You cannot modify your own role.");
    }

    [Fact]
    public async Task ApproveDoctorAsync_PendingDoctor_VerifiesDoctor()
    {
        // Arrange
        var doctorId = "doc-1";
        var doctor = new DoctorProfile { Id = doctorId, IsVerified = false };
        _doctorRepoMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);

        // Act
        var result = await _sut.ApproveDoctorAsync(doctorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        doctor.IsVerified.Should().BeTrue();
        _doctorRepoMock.Verify(x => x.UpdateAsync(doctor), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
