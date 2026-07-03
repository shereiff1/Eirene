using Eirene.BLL.Services.Implementation.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eirene.UnitTests.BLL.Services;

public class FileUploadServiceTests
{
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<LocalPictureService>> _loggerMock;
    private readonly LocalPictureService _sut;

    public FileUploadServiceTests()
    {
        _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<LocalPictureService>>();

        _webHostEnvironmentMock.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());

        _sut = new LocalPictureService(
            _webHostEnvironmentMock.Object,
            _configurationMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task UploadPictureAsync_FileNullOrEmpty_ReturnsFailure()
    {
        // Act - Null File
        var resultNull = await _sut.UploadPictureAsync(null!);
        
        // Act - Empty File
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        var resultEmpty = await _sut.UploadPictureAsync(fileMock.Object);

        // Assert
        resultNull.IsSuccess.Should().BeFalse();
        resultNull.Error.Should().Be("No file uploaded.");

        resultEmpty.IsSuccess.Should().BeFalse();
        resultEmpty.Error.Should().Be("No file uploaded.");
    }

    [Fact]
    public async Task UploadPictureAsync_InvalidExtension_ReturnsFailure()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.FileName).Returns("document.pdf"); // Not jpg, jpeg, png, gif

        // Act
        var result = await _sut.UploadPictureAsync(fileMock.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid file type. Only jpg, jpeg, png, gif are allowed.");
    }

    [Fact]
    public async Task UploadPictureAsync_FileTooLarge_ReturnsFailure()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB, exceeds 5MB limit
        fileMock.Setup(f => f.FileName).Returns("photo.png");

        // Act
        var result = await _sut.UploadPictureAsync(fileMock.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("File size exceeds the 5MB limit.");
    }

    [Fact]
    public async Task UploadPictureAsync_ValidFile_ReturnsSuccessAndUrl()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1 * 1024 * 1024); // 1MB
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _configurationMock.Setup(c => c["Storage:Local:ProfilesPath"]).Returns("images/profiles");

        // Act
        var result = await _sut.UploadPictureAsync(fileMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Url.Should().Contain("images/profiles");
        result.Url.Should().EndWith(".jpg");
        result.Error.Should().BeNull();

        // Cleanup test directory if created
        var expectedFolder = Path.Combine(Directory.GetCurrentDirectory(), "images\\profiles");
        if (Directory.Exists(expectedFolder))
        {
            try
            {
                Directory.Delete(expectedFolder, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
