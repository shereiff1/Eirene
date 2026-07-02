using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Eirene.BLL.Models.Common;
using Eirene.BLL.ModelVMs.Content;
using Eirene.BLL.Services.Implementation.Content;
using Eirene.DAL.Entities.Content;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Content;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Content;

public class BlogServicesTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IBlogRepository> _blogRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<HybridCache> _cacheMock;
    private readonly Mock<ILogger<BlogServices>> _loggerMock;
    private readonly BlogServices _sut;

    public BlogServicesTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _blogRepoMock = new Mock<IBlogRepository>();
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheMock = new Mock<HybridCache>();
        _loggerMock = new Mock<ILogger<BlogServices>>();

        _sut = new BlogServices(
            _blogRepoMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object
        );
    }

    private void SetupCache<T>()
    {
        _cacheMock.Setup(x => x.GetOrCreateAsync<Func<CancellationToken, ValueTask<T>>, T>(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<T>>>(),
            It.IsAny<Func<Func<CancellationToken, ValueTask<T>>, CancellationToken, ValueTask<T>>>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (string key, Func<CancellationToken, ValueTask<T>> state, Func<Func<CancellationToken, ValueTask<T>>, CancellationToken, ValueTask<T>> factory, HybridCacheEntryOptions options, IEnumerable<string> tags, CancellationToken token) =>
            {
                return await factory(state, token);
            });
    }

    // ========== GetAllAsync ==========

    [Fact]
    public async Task GetAllAsync_BlogsExist_ReturnsSuccess()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var blogs = _fixture.CreateMany<Blog>(3).ToList();
        var blogDTOs = _fixture.CreateMany<BlogDTO>(3).ToList();
        
        _blogRepoMock.Setup(x => x.GetAllPagedAsync(page, pageSize))
            .ReturnsAsync((blogs, 3));
        _mapperMock.Setup(x => x.Map<List<BlogDTO>>(blogs))
            .Returns(blogDTOs);

        // Act
        var (isSuccess, posts) = await _sut.GetAllAsync(page, pageSize);

        // Assert
        isSuccess.Should().BeTrue();
        Assert.NotNull(posts);
        posts!.Items.Should().HaveCount(3);
        posts.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_NoBlogs_ReturnsFailure()
    {
        // Arrange
        _blogRepoMock.Setup(x => x.GetAllPagedAsync(1, 10))
            .ReturnsAsync((null!, 0));

        // Act
        var (isSuccess, posts) = await _sut.GetAllAsync(1, 10);

        // Assert
        isSuccess.Should().BeFalse();
        Assert.Null(posts);
    }

    [Fact]
    public async Task GetAllAsync_ExceptionThrown_ReturnsFailureAndLogs()
    {
        // Arrange
        _blogRepoMock.Setup(x => x.GetAllPagedAsync(1, 10))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var (isSuccess, posts) = await _sut.GetAllAsync(1, 10);

        // Assert
        isSuccess.Should().BeFalse();
        Assert.Null(posts);
    }

    // ========== GetByDoctorIdAsync ==========

    [Fact]
    public async Task GetByDoctorIdAsync_BlogsExist_ReturnsSuccess()
    {
        // Arrange
        var doctorId = "doc-1";
        var blogs = _fixture.CreateMany<Blog>(2).ToList();
        var blogDTOs = _fixture.CreateMany<BlogDTO>(2).ToList();

        SetupCache<List<BlogDTO>>();

        _blogRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Blog, bool>>>()))
            .ReturnsAsync(blogs);
        _mapperMock.Setup(x => x.Map<List<BlogDTO>>(blogs))
            .Returns(blogDTOs);

        // Act
        var (isSuccess, posts) = await _sut.GetByDoctorIdAsync(doctorId);

        // Assert
        isSuccess.Should().BeTrue();
        Assert.NotNull(posts);
        posts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDoctorIdAsync_CacheReturnsNull_ReturnsFailure()
    {
        // Arrange
        var doctorId = "doc-1";
        SetupCache<List<BlogDTO>?>();

        _blogRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Blog, bool>>>()))
            .ReturnsAsync((List<Blog>?)null!);

        // Act
        var (isSuccess, posts) = await _sut.GetByDoctorIdAsync(doctorId);

        // Assert
        isSuccess.Should().BeFalse();
        Assert.Null(posts);
    }

    [Fact]
    public async Task GetByDoctorIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var doctorId = "doc-1";
        _cacheMock.Setup(x => x.GetOrCreateAsync<Func<CancellationToken, ValueTask<List<BlogDTO>>>, List<BlogDTO>>(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<List<BlogDTO>>>>(),
            It.IsAny<Func<Func<CancellationToken, ValueTask<List<BlogDTO>>>, CancellationToken, ValueTask<List<BlogDTO>>>>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache failure"));

        // Act
        var (isSuccess, posts) = await _sut.GetByDoctorIdAsync(doctorId);

        // Assert
        isSuccess.Should().BeFalse();
        Assert.Null(posts);
    }

    // ========== GetByIdAsync ==========

    [Fact]
    public async Task GetByIdAsync_BlogExists_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var blog = _fixture.Create<Blog>();
        var blogDTO = _fixture.Create<BlogDTO>();

        SetupCache<BlogDTO>();

        _blogRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(blog);
        _mapperMock.Setup(x => x.Map<BlogDTO>(blog)).Returns(blogDTO);

        // Act
        var (isSuccess, post) = await _sut.GetByIdAsync(id);

        // Assert
        isSuccess.Should().BeTrue();
        Assert.NotNull(post);
    }

    [Fact]
    public async Task GetByIdAsync_CacheReturnsNull_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        SetupCache<BlogDTO?>();

        _blogRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Blog)null!);

        // Act
        var (isSuccess, post) = await _sut.GetByIdAsync(id);

        // Assert
        isSuccess.Should().BeFalse();
        Assert.Null(post);
    }

    // ========== CreateAsync ==========

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedBlog()
    {
        // Arrange
        var doctorId = "doc-1";
        var addModel = _fixture.Create<AddBlog>();
        var blog = _fixture.Create<Blog>();
        var createdBlog = _fixture.Create<Blog>();
        var blogDTO = _fixture.Create<BlogDTO>();

        _mapperMock.Setup(x => x.Map<Blog>(addModel)).Returns(blog);
        _blogRepoMock.Setup(x => x.AddAsync(blog)).ReturnsAsync(createdBlog);
        _mapperMock.Setup(x => x.Map<BlogDTO>(createdBlog)).Returns(blogDTO);

        // Act
        var (isSuccess, createdPost) = await _sut.CreateAsync(addModel, doctorId);

        // Assert
        isSuccess.Should().BeTrue();
        Assert.NotNull(createdPost);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync($"doctor-blogs-{doctorId}", default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_RepoAddFails_ReturnsFailure()
    {
        // Arrange
        var doctorId = "doc-1";
        var addModel = _fixture.Create<AddBlog>();
        var blog = _fixture.Create<Blog>();

        _mapperMock.Setup(x => x.Map<Blog>(addModel)).Returns(blog);
        _blogRepoMock.Setup(x => x.AddAsync(blog)).ReturnsAsync((Blog)null!);

        // Act
        var (isSuccess, createdPost) = await _sut.CreateAsync(addModel, doctorId);

        // Assert
        isSuccess.Should().BeFalse();
        Assert.Null(createdPost);
    }

    // ========== UpdateAsync ==========

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var doctorId = "doc-1";
        var editModel = _fixture.Build<EditBlog>().With(x => x.DoctorId, doctorId).Create();
        var blog = _fixture.Build<Blog>().With(x => x.DoctorId, doctorId).Create();

        _blogRepoMock.Setup(x => x.GetByIdAsync(editModel.Id)).ReturnsAsync(blog);
        _blogRepoMock.Setup(x => x.UpdateAsync(blog)).ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateAsync(editModel);

        // Assert
        result.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync($"blog-{editModel.Id}", default), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync($"doctor-blogs-{doctorId}", default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_BlogNotFound_ReturnsFalse()
    {
        // Arrange
        var editModel = _fixture.Create<EditBlog>();
        _blogRepoMock.Setup(x => x.GetByIdAsync(editModel.Id)).ReturnsAsync((Blog)null!);

        // Act
        var result = await _sut.UpdateAsync(editModel);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_UnauthorizedDoctor_ReturnsFalse()
    {
        // Arrange
        var editModel = _fixture.Build<EditBlog>().With(x => x.DoctorId, "doc-1").Create();
        var blog = _fixture.Build<Blog>().With(x => x.DoctorId, "other-doc").Create();

        _blogRepoMock.Setup(x => x.GetByIdAsync(editModel.Id)).ReturnsAsync(blog);

        // Act
        var result = await _sut.UpdateAsync(editModel);

        // Assert
        result.Should().BeFalse();
    }

    // ========== DeleteAsync ==========

    [Fact]
    public async Task DeleteAsync_BlogExists_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var blog = _fixture.Build<Blog>().With(x => x.DoctorId, "doc-1").Create();

        _blogRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(blog);
        _blogRepoMock.Setup(x => x.DeleteAsync(blog)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync($"blog-{id}", default), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync($"doctor-blogs-doc-1", default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_BlogNotFound_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _blogRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Blog)null!);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeFalse();
    }
}
