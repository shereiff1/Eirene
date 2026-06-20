using AutoFixture;
using AutoMapper;
using Eirene.BLL.Models.Identity;
using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Implementation.Identity;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Eirene.Tests.Shared.MockHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Eirene.UnitTests.BLL.Services.Identity;

public class AuthServicesTests
{
    private readonly IFixture _fixture;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<AuthServices>> _loggerMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly AuthServices _sut;

    public AuthServicesTests()
    {
        _fixture = new Fixture();
        
        // Circular reference handling for AutoFixture
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _userManagerMock = IdentityMockHelpers.MockUserManager();
        _signInManagerMock = IdentityMockHelpers.MockSignInManager(_userManagerMock);
        _roleManagerMock = IdentityMockHelpers.MockRoleManager();

        _tokenServiceMock = new Mock<ITokenService>();
        _mapperMock = new Mock<IMapper>();
        _emailSenderMock = new Mock<IEmailSender>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthServices>>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _configurationMock.Setup(c => c["Security:OtpSecretKey"]).Returns("TestSecretKeyThatIsAtLeast16CharsLong");
        _configurationMock.Setup(c => c["Google:WebClientId"]).Returns("GoogleWebClientId");
        
        _sut = new AuthServices(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _roleManagerMock.Object,
            _tokenServiceMock.Object,
            _mapperMock.Object,
            _emailSenderMock.Object,
            _refreshTokenRepoMock.Object,
            _unitOfWorkMock.Object,
            _configurationMock.Object,
            _loggerMock.Object,
            _backgroundJobServiceMock.Object,
            _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = _fixture.Build<RegisterDTO>()
            .With(x => x.Role, "Patient")
            .Create();
        var user = _fixture.Create<ApplicationUser>();

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(x => x.FindByNameAsync(request.UserName)).ReturnsAsync((ApplicationUser?)null);
        _mapperMock.Setup(x => x.Map<ApplicationUser>(request)).Returns(user);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password)).ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(request.Role)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, request.Role)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().NotBeNullOrEmpty();
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password), Times.Once);
        _backgroundJobServiceMock.Verify(x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var request = _fixture.Create<RegisterDTO>();
        var existingUser = _fixture.Create<ApplicationUser>();

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(existingUser);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("CONFLICT");
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsFailure()
    {
        // Arrange
        var request = _fixture.Create<RegisterDTO>();
        var existingUser = _fixture.Create<ApplicationUser>();

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(x => x.FindByNameAsync(request.UserName)).ReturnsAsync(existingUser);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("USERNAME_CONFLICT");
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_InvalidRole_ReturnsFailure()
    {
        // Arrange
        var request = _fixture.Build<RegisterDTO>()
            .With(x => x.Role, "SuperAdmin") // Invalid role
            .Create();
        var user = _fixture.Create<ApplicationUser>();

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(x => x.FindByNameAsync(request.UserName)).ReturnsAsync((ApplicationUser?)null);
        _mapperMock.Setup(x => x.Map<ApplicationUser>(request)).Returns(user);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_ROLE");
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var request = _fixture.Create<LoginDTO>();
        var user = _fixture.Build<ApplicationUser>().With(x => x.EmailConfirmed, true).Create();
        var authResultDto = _fixture.Build<AuthResultDTO>().With(x => x.Success, true).Create();
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(user, request.Password, request.RememberMe, true))
            .ReturnsAsync(SignInResult.Success);
        
        _tokenServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
            .ReturnsAsync(("access_token", "jti", DateTime.UtcNow.AddHours(1)));
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Patient" });
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");
        _tokenServiceMock.Setup(x => x.ComputeSha256Hash("refresh_token")).Returns("hashed_rt");
        
        _mapperMock.Setup(x => x.Map<AuthResultDTO>(user)).Returns(authResultDto);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.Role.Should().Be("Patient");
        
        _refreshTokenRepoMock.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt => rt.TokenHash == "hashed_rt" && rt.UserId == user.Id)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ReturnsFailure()
    {
        // Arrange
        var request = _fixture.Create<LoginDTO>();
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_EmailNotConfirmed_ReturnsFailure()
    {
        // Arrange
        var request = _fixture.Create<LoginDTO>();
        var user = _fixture.Build<ApplicationUser>().With(x => x.EmailConfirmed, false).Create();
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Email not confirmed");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // Arrange
        var request = _fixture.Create<LoginDTO>();
        var user = _fixture.Build<ApplicationUser>().With(x => x.EmailConfirmed, true).Create();
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(user, request.Password, request.RememberMe, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task ConfirmEmailCodeAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var email = "nonexistent@test.com";
        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _sut.ConfirmEmailCodeAsync(email, "123456");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task ConfirmEmailCodeAsync_CodeExpired_ReturnsFailure()
    {
        // Arrange
        var email = "test@test.com";
        var user = new ApplicationUser 
        { 
            Email = email, 
            EmailVerificationCodeExpiration = DateTime.UtcNow.AddMinutes(-1) 
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);

        // Act
        var result = await _sut.ConfirmEmailCodeAsync(email, "123456");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("The confirmation code has expired");
        result.ErrorCode.Should().Be("EXPIRED_CODE");
    }
}
