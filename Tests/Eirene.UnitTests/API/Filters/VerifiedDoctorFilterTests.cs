using AutoFixture;
using AutoFixture.AutoMoq;
using Eirene.API.Filters;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Eirene.UnitTests.API.Filters;

public class VerifiedDoctorFilterTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IDoctorProfileRepository> _doctorRepoMock;
    private readonly VerifiedDoctorFilter _sut;

    public VerifiedDoctorFilterTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _doctorRepoMock = new Mock<IDoctorProfileRepository>();
        _sut = new VerifiedDoctorFilter(_doctorRepoMock.Object);
    }

    [Fact]
    public async Task OnActionExecutionAsync_NoUserId_ReturnsUnauthorized()
    {
        // Arrange
        var context = CreateActionExecutingContext(new ClaimsPrincipal()); // No claims
        var nextMock = new Mock<ActionExecutionDelegate>();

        // Act
        await _sut.OnActionExecutionAsync(context, nextMock.Object);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
        nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_DoctorNotFound_ReturnsForbidden()
    {
        // Arrange
        var userId = "doc-1";
        var context = CreateActionExecutingContext(CreatePrincipal(userId));
        var nextMock = new Mock<ActionExecutionDelegate>();

        _doctorRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((DoctorProfile?)null);

        // Act
        await _sut.OnActionExecutionAsync(context, nextMock.Object);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(403, result.StatusCode);
        nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_DoctorNotVerified_ReturnsForbidden()
    {
        // Arrange
        var userId = "doc-1";
        var context = CreateActionExecutingContext(CreatePrincipal(userId));
        var nextMock = new Mock<ActionExecutionDelegate>();

        var doctor = new DoctorProfile { IsVerified = false };

        _doctorRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(doctor);

        // Act
        await _sut.OnActionExecutionAsync(context, nextMock.Object);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(403, result.StatusCode);
        nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_DoctorVerified_CallsNext()
    {
        // Arrange
        var userId = "doc-1";
        var context = CreateActionExecutingContext(CreatePrincipal(userId));
        var nextMock = new Mock<ActionExecutionDelegate>();

        var doctor = new DoctorProfile { IsVerified = true };

        _doctorRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(doctor);

        // Act
        await _sut.OnActionExecutionAsync(context, nextMock.Object);

        // Assert
        Assert.Null(context.Result);
        nextMock.Verify(x => x(), Times.Once);
    }

    private static ClaimsPrincipal CreatePrincipal(string nameIdentifier)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, nameIdentifier)
        });
        return new ClaimsPrincipal(identity);
    }

    private static ActionExecutingContext CreateActionExecutingContext(ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext { User = principal };
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor()
        );

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new Mock<Controller>().Object
        );
    }
}
