using BLL.Models.Identity;
using BLL.Services.Abstraction.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;

namespace Eirene.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthServices authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _authService.LogoutAsync(userId);
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailCode confirmEmailCode)
        {
            var result = await _authService.ConfirmEmailCodeAsync(confirmEmailCode.Email, confirmEmailCode.Code);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("get-code")]
        public async Task<IActionResult> GetCode([FromBody] ResendCodeDTO resendCodeDTO)
        {
            var result = await _authService.ResendVerificationCodeAsync(resendCodeDTO.Email);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}