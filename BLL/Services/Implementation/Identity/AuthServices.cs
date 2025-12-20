using AutoMapper;
using BLL.Models.Identity;
using BLL.Services.Abstraction.Identity;
using DAL.Entities.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using DAL.Repository.Abstraction.Core;

namespace BLL.Services.Implementation.Identity
{
    public class AuthServices : IAuthServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthServices> _logger;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthServices(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IMapper mapper,
            IEmailSender emailSender,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<AuthServices> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _emailSender = emailSender;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task<AuthResultDTO> RegisterAsync(RegisterDTO registerDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return new AuthResultDTO
                    {
                        Success = false,
                        Errors = new[] { "User with this email already exists" }
                    };
                }

                var user = _mapper.Map<ApplicationUser>(registerDto);

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    return new AuthResultDTO
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToArray()
                    };
                }

                string role = registerDto.Role ?? "Patient";

                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);

                var code = new Random().Next(100000, 999999).ToString();

                await _userManager.UpdateAsync(user);
                await _emailSender.SendEmailAsync(user.Email, "Verification Code",
                    $"Your verification code is: {code}");

                var authResult = _mapper.Map<AuthResultDTO>(user);
                authResult.Code = code;
                authResult.Success = true;
                authResult.Message = "Registration successful. Verification code sent to your email.";

                return authResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterAsync failed");
                return new AuthResultDTO
                {
                    Success = false,
                    Errors = new[] { $"An error occurred during registration: {ex.Message}" }
                };
            }
        }

        public async Task<AuthResultDTO> LoginAsync(LoginDTO loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                    return Fail("Invalid email or password");

                if (!user.EmailConfirmed)
                    return Fail("Email not confirmed");

                var result = await _signInManager.PasswordSignInAsync(
                    user, loginDto.Password, loginDto.RememberMe, true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                        return Fail("Account is locked out");

                    if (result.IsNotAllowed)
                        return Fail("Login not allowed");

                    return Fail("Invalid email or password");
                }

                var (accessToken, jti, expiry) = await _tokenService.GenerateJwtTokenAsync(user);
                var roles = await _userManager.GetRolesAsync(user);

                var refreshToken = _tokenService.GenerateRefreshToken();
                var refreshTokenHash = _tokenService.ComputeSha256Hash(refreshToken);

                await _refreshTokenRepository.AddAsync(new RefreshToken
                {
                    TokenHash = refreshTokenHash,
                    JwtId = jti,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedDate = DateTime.UtcNow
                });
                var authResult = _mapper.Map<AuthResultDTO>(user);
                authResult.RefreshToken = refreshTokenHash;
                authResult.AccessToken = accessToken;
                authResult.Success = true;
                return authResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync failed for email: {Email}", loginDto.Email);
                return new AuthResultDTO
                {
                    Success = false,
                    Errors = new[] { $"An error occurred during login: {ex.Message}" }
                };
            }
        }

        public async Task<ConfirmMailDTO> ConfirmEmailCodeAsync(ConfirmEmailCode dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return new ConfirmMailDTO
                    {
                        Success = false,
                        Message =   "User not found"
                    };
                }

                if (!dto.IsConfirmed)
                {
                    return new ConfirmMailDTO
                    {
                        Success = false,
                        Message = "The confirmation code is not correct" 
                    };
                }

                user.EmailConfirmed = true;
                user.IsEmailVerified = true;
                await _userManager.UpdateAsync(user);

                return new ConfirmMailDTO
                {
                    Success = true,
                    Message = "Email confirmed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmEmailCodeAsync failed");
                return new ConfirmMailDTO
                {
                    Success = false,
                    Message = $"An error occurred during email confirmation: {ex.Message}" 
                };
            }
        }

        public async Task LogoutAsync(string userId)
        {
            try
            {
                await _signInManager.SignOutAsync();

                var tokens = await _refreshTokenRepository
                    .FindAsync(rt => rt.UserId == userId && !rt.IsRevoked);

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    await _refreshTokenRepository.UpdateAsync(token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogoutAsync failed for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<AuthResultDTO> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromToken(accessToken);
                if (principal == null)
                    return Fail("Invalid access token");

                var userId = _tokenService.GetUserIdFromPrincipal(principal);
                var jti = _tokenService.GetJtiFromPrincipal(principal);

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(jti))
                    return Fail("Invalid token claims");

                var hashed = _tokenService.ComputeSha256Hash(refreshToken);

                var storedToken = (await _refreshTokenRepository
                        .FindAsync(rt => rt.TokenHash == hashed))
                    .SingleOrDefault();

                if (storedToken == null ||
                    storedToken.IsUsed ||
                    storedToken.IsRevoked ||
                    storedToken.ExpiryDate < DateTime.UtcNow ||
                    storedToken.JwtId != jti)
                {
                    if (storedToken != null && storedToken.IsUsed)
                    {
                        await RevokeAllRefreshTokensForUser(userId);
                    }

                    return Fail("Invalid refresh token");
                }

                storedToken.IsUsed = true;
                await _refreshTokenRepository.UpdateAsync(storedToken);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Fail("User not found");
                var (newAccessToken, newJti, _) = await _tokenService.GenerateJwtTokenAsync(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                var newRefreshTokenHash = _tokenService.ComputeSha256Hash(newRefreshToken);

                await _refreshTokenRepository.AddAsync(new RefreshToken
                {
                    TokenHash = newRefreshTokenHash,
                    JwtId = newJti,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedDate = DateTime.UtcNow
                });

                return new AuthResultDTO
                {
                    Success = true,
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    Message = "Token refreshed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshTokenAsync failed");
                return new AuthResultDTO
                {
                    Success = false,
                    Errors = new[] { $"An error occurred during token refresh: {ex.Message}" }
                };
            }
        }

        public async Task<AuthResultDTO> ResendVerificationCodeAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return Fail("User not found");

                if (user.EmailConfirmed)
                    return Fail("Email is already confirmed");

                var code = new Random().Next(100000, 999999).ToString();

                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Email Verification Code",
                    $"Your verification code is: {code}"
                );

                return new AuthResultDTO
                {
                    Success = true,
                    Code = code,
                    Message = "Verification code sent successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResendVerificationCodeAsync failed");
                return new AuthResultDTO
                {
                    Success = false,
                    Errors = new[] { "Failed to send verification code" }
                };
            }
        }


        private async Task RevokeAllRefreshTokensForUser(string userId)
        {
            try
            {
                var tokens = await _refreshTokenRepository
                    .FindAsync(rt => rt.UserId == userId && (!rt.IsRevoked || !rt.IsUsed));

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    await _refreshTokenRepository.UpdateAsync(token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RevokeAllRefreshTokensForUser failed for user {UserId}", userId);
            }
        }

        private static AuthResultDTO Fail(string error) =>
            new() { Success = false, Errors = new[] { error } };
    }
}