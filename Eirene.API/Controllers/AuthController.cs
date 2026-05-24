using Eirene.BLL.Models.Identity;
using Eirene.BLL.Services.Abstraction.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserContext _userContext;

    public AuthController(IAuthServices authService, ILogger<AuthController> logger, IUserContext userContext)
    {
        _authService = authService;
        _logger = logger;
        _userContext = userContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(registerDto);

        if (!result.Success)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(loginDto);

        if (!result.Success)
            return Unauthorized(new { message = result.Error });

        return Ok(result);
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO googleLoginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.GoogleLoginAsync(googleLoginDto);

        if (!result.Success)
            return Unauthorized(new { message = result.Error });

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        await _authService.LogoutAsync(userId);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailCode confirmEmailCode)
    {
        var result = await _authService.ConfirmEmailCodeAsync(confirmEmailCode.Email, confirmEmailCode.Code);

        if (!result.Success)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { message = result.Message });

            // INVALID_CODE or EXPIRED_CODE
            return UnprocessableEntity(new { message = result.Message });
        }

        return Ok(result);
    }

    [HttpPost("get-code")]
    public async Task<IActionResult> GetCode([FromBody] ResendCodeDTO resendCodeDTO)
    {
        var result = await _authService.ResendVerificationCodeAsync(resendCodeDTO.Email);

        if (!result.Success)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { message = result.Error });
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO tokenRequestDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RefreshTokenAsync(tokenRequestDto.AccessToken, tokenRequestDto.RefreshToken);

        if (!result.Success)
            return Unauthorized(new { message = result.Error });

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

        if (!result.Success)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ResetPasswordAsync(resetPasswordDto);

        if (!result.Success)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { message = result.Error });
            if (result.ErrorCode == "EXPIRED_CODE" || result.ErrorCode == "INVALID_CODE")
                return UnprocessableEntity(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return Ok(result);
    }
}
