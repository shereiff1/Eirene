using AutoMapper;
using Eirene.BLL.Models.Community.Post;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Implementation.Community;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Community;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Community;

public class CommunityPostServicesTests
{
    private readonly Mock<ICommunityPostRepository> _postRepoMock;
    private readonly Mock<ICommunityGroupRepository> _groupRepoMock;
    private readonly Mock<IUserCommunityGroupRepository> _userGroupRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CommunityPostServices>> _loggerMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IContentModerationService> _moderationMock;
    private readonly CommunityPostServices _sut;

    public CommunityPostServicesTests()
    {
        _postRepoMock = new Mock<ICommunityPostRepository>();
        _groupRepoMock = new Mock<ICommunityGroupRepository>();
        _userGroupRepoMock = new Mock<IUserCommunityGroupRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CommunityPostServices>>();
        _userContextMock = new Mock<IUserContext>();
        _moderationMock = new Mock<IContentModerationService>();

        _sut = new CommunityPostServices(
            _loggerMock.Object,
            _mapperMock.Object,
            _postRepoMock.Object,
            _groupRepoMock.Object,
            _userGroupRepoMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _moderationMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ValidMember_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-1";
        var groupId = Guid.NewGuid();
        var model = new AddCommunityPost { CommunityGroupId = groupId, Content = "Hello" };
        var group = new CommunityGroup { Id = groupId };
        var post = new CommunityPost { CommunityGroupId = groupId, UserId = userId, Content = "Hello" };

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _groupRepoMock.Setup(x => x.GetByIdAsync(groupId)).ReturnsAsync(group);
        _userGroupRepoMock.Setup(x => x.GetByGroupAndUserAsync(groupId, userId)).ReturnsAsync(new UserCommunityGroup());
        _moderationMock.Setup(x => x.ModerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(ContentModerationResult.Allowed());
        _mapperMock.Setup(x => x.Map<CommunityPost>(model)).Returns(post);
        _postRepoMock.Setup(x => x.AddAsync(post)).ReturnsAsync(post);
        _postRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync(post);
        _mapperMock.Setup(x => x.Map<CommunityPostDTO>(post)).Returns(new CommunityPostDTO());

        // Act
        var result = await _sut.CreateAsync(model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NotMember_ReturnsFailure()
    {
        // Arrange
        var userId = "user-1";
        var groupId = Guid.NewGuid();
        var model = new AddCommunityPost { CommunityGroupId = groupId, Content = "Hello" };
        var group = new CommunityGroup { Id = groupId };

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _groupRepoMock.Setup(x => x.GetByIdAsync(groupId)).ReturnsAsync(group);
        _userGroupRepoMock.Setup(x => x.GetByGroupAndUserAsync(groupId, userId)).ReturnsAsync((UserCommunityGroup)null!);

        // Act
        var result = await _sut.CreateAsync(model);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("join this community group");
    }

    [Fact]
    public async Task UpdateAsync_Owner_ReturnsTrue()
    {
        // Arrange
        var userId = "user-1";
        var postId = Guid.NewGuid();
        var post = new CommunityPost { Id = postId, UserId = userId, Content = "Old" };
        var model = new EditCommunityPost { Id = postId, Content = "New" };

        _postRepoMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _postRepoMock.Setup(x => x.UpdateAsync(post)).ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateAsync(model);

        // Assert
        result.Should().BeTrue();
        post.Content.Should().Be("New");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ReturnsFalse()
    {
        // Arrange
        var userId = "user-1";
        var postId = Guid.NewGuid();
        var post = new CommunityPost { Id = postId, UserId = "other-user" };
        var model = new EditCommunityPost { Id = postId };

        _postRepoMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _sut.UpdateAsync(model);

        // Assert
        result.Should().BeFalse();
    }
}
