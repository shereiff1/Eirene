using AutoMapper;
using Eirene.BLL.Models.Community.Group;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Implementation.Community;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Community;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Community;

public class CommunityGroupServicesTests
{
    private readonly Mock<ICommunityGroupRepository> _groupRepoMock;
    private readonly Mock<IUserCommunityGroupRepository> _userGroupRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CommunityGroupServices>> _loggerMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<HybridCache> _cacheMock;
    private readonly CommunityGroupServices _sut;

    public CommunityGroupServicesTests()
    {
        _groupRepoMock = new Mock<ICommunityGroupRepository>();
        _userGroupRepoMock = new Mock<IUserCommunityGroupRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CommunityGroupServices>>();
        _userContextMock = new Mock<IUserContext>();
        _cacheMock = new Mock<HybridCache>();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new CommunityGroupServices(
            _loggerMock.Object,
            _mapperMock.Object,
            _groupRepoMock.Object,
            _userGroupRepoMock.Object,
            _unitOfWorkMock.Object,
            _userManagerMock.Object,
            _userContextMock.Object,
            _cacheMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsGroup()
    {
        // Arrange
        var userId = "user-1";
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.IsInRole(Eirene.BLL.Enumerators.Roles.Admin)).Returns(true);
        var model = new AddCommunityGroup { Name = "New Group" };
        var entity = new CommunityGroup { Name = "New Group", CreatedByUserId = userId };
        var dto = new CommunityGroupDTO { Name = "New Group" };

        _mapperMock.Setup(x => x.Map<CommunityGroup>(model)).Returns(entity);
        _groupRepoMock.Setup(x => x.AddAsync(entity)).ReturnsAsync(entity);
        _groupRepoMock.Setup(x => x.GetByIdWithDetailsAsync(entity.Id)).ReturnsAsync(entity);
        _mapperMock.Setup(x => x.Map<CommunityGroupDTO>(entity)).Returns(dto);

        // Act
        var result = await _sut.CreateAsync(model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Assert.NotNull(result.CreatedGroup);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task JoinGroupAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = "user-1";
        var group = new CommunityGroup { Id = groupId };

        _groupRepoMock.Setup(x => x.GetByIdAsync(groupId)).ReturnsAsync(group);
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(new ApplicationUser());
        _userGroupRepoMock.Setup(x => x.GetByGroupAndUserAsync(groupId, userId)).ReturnsAsync((UserCommunityGroup)null!);

        // Act
        var result = await _sut.JoinGroupAsync(groupId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userGroupRepoMock.Verify(x => x.AddAsync(It.IsAny<UserCommunityGroup>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task JoinGroupAsync_AlreadyJoined_ReturnsFailure()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = "user-1";
        var group = new CommunityGroup { Id = groupId };

        _groupRepoMock.Setup(x => x.GetByIdAsync(groupId)).ReturnsAsync(group);
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(new ApplicationUser());
        _userGroupRepoMock.Setup(x => x.GetByGroupAndUserAsync(groupId, userId)).ReturnsAsync(new UserCommunityGroup());

        // Act
        var result = await _sut.JoinGroupAsync(groupId, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("You are already a member of this group.");
    }

    [Fact]
    public async Task DeleteAsync_Unauthorized_ReturnsFalse()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = "user-1";
        var group = new CommunityGroup { Id = groupId, CreatedByUserId = "other-user" };

        _groupRepoMock.Setup(x => x.GetByIdAsync(groupId)).ReturnsAsync(group);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _sut.DeleteAsync(groupId);

        // Assert
        result.Should().BeFalse();
        _groupRepoMock.Verify(x => x.DeleteAsync(It.IsAny<CommunityGroup>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Creator_ReturnsTrue()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = "user-1";
        var group = new CommunityGroup { Id = groupId, CreatedByUserId = userId };

        _groupRepoMock.Setup(x => x.GetByIdAsync(groupId)).ReturnsAsync(group);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _groupRepoMock.Setup(x => x.DeleteAsync(group)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(groupId);

        // Assert
        result.Should().BeTrue();
        _groupRepoMock.Verify(x => x.DeleteAsync(group), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
