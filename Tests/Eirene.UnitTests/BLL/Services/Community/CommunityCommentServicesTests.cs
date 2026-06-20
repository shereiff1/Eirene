using AutoMapper;
using Eirene.BLL.Models.Community.Comment;
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

public class CommunityCommentServicesTests
{
    private readonly Mock<ICommunityCommentRepository> _commentRepoMock;
    private readonly Mock<ICommunityPostRepository> _postRepoMock;
    private readonly Mock<IUserCommunityGroupRepository> _userGroupRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CommunityCommentServices>> _loggerMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IContentModerationService> _moderationMock;
    private readonly CommunityCommentServices _sut;

    public CommunityCommentServicesTests()
    {
        _commentRepoMock = new Mock<ICommunityCommentRepository>();
        _postRepoMock = new Mock<ICommunityPostRepository>();
        _userGroupRepoMock = new Mock<IUserCommunityGroupRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CommunityCommentServices>>();
        _userContextMock = new Mock<IUserContext>();
        _moderationMock = new Mock<IContentModerationService>();

        _sut = new CommunityCommentServices(
            _loggerMock.Object,
            _mapperMock.Object,
            _commentRepoMock.Object,
            _postRepoMock.Object,
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
        var postId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var model = new AddCommunityComment { PostId = postId, Content = "Nice post" };
        var post = new CommunityPost { Id = postId, CommunityGroupId = groupId };
        var comment = new CommunityComment { PostId = postId, UserId = userId, Content = "Nice post" };

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _postRepoMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);
        _userGroupRepoMock.Setup(x => x.GetByGroupAndUserAsync(groupId, userId)).ReturnsAsync(new UserCommunityGroup());
        _moderationMock.Setup(x => x.ModerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(ContentModerationResult.Allowed());
        _mapperMock.Setup(x => x.Map<CommunityComment>(model)).Returns(comment);
        _commentRepoMock.Setup(x => x.AddAsync(comment)).ReturnsAsync(comment);
        _commentRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync(comment);
        _mapperMock.Setup(x => x.Map<CommunityCommentDTO>(comment)).Returns(new CommunityCommentDTO());

        // Act
        var result = await _sut.CreateAsync(model);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteAsync_Owner_ReturnsTrue()
    {
        // Arrange
        var userId = "user-1";
        var commentId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var comment = new CommunityComment { Id = commentId, UserId = userId, PostId = postId };

        _commentRepoMock.Setup(x => x.GetByIdAsync(commentId)).ReturnsAsync(comment);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _commentRepoMock.Setup(x => x.UpdateAsync(comment)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(commentId);

        // Assert
        result.Should().BeTrue();
        _commentRepoMock.Verify(x => x.UpdateAsync(comment), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ReturnsFalse()
    {
        // Arrange
        var userId = "user-1";
        var commentId = Guid.NewGuid();
        var comment = new CommunityComment { Id = commentId, UserId = "other-user" };
        var model = new EditCommunityComment { Id = commentId, Content = "Edited" };

        _commentRepoMock.Setup(x => x.GetByIdAsync(commentId)).ReturnsAsync(comment);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _sut.UpdateAsync(model);

        // Assert
        result.Should().BeFalse();
    }
}
