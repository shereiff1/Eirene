using AutoMapper;
using Eirene.BLL.Models.Identity;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Eirene.DAL.Repository.Abstraction;
using Google.Apis.Auth;

namespace Eirene.BLL.Services.Implementation.Identity;

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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _otpSecret;
    private readonly string? _googleWebClientId;
    private readonly string? _googleIosClientId;
    private readonly string? _googleAndroidClientId;

    public AuthServices(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        IMapper mapper,
        IEmailSender emailSender,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthServices> logger,
        IBackgroundJobService backgroundJobService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _mapper = mapper;
        _emailSender = emailSender;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _otpSecret = configuration["Security:OtpSecretKey"] ?? throw new InvalidOperationException("Security:OtpSecretKey is missing");
        _backgroundJobService = backgroundJobService;
        _httpContextAccessor = httpContextAccessor;
        _googleWebClientId = configuration["Google:WebClientId"];
        _googleIosClientId = configuration["Google:IosClientId"];
        _googleAndroidClientId = configuration["Google:AndroidClientId"];
    }

    public async Task<RegistrationDTO> RegisterAsync(RegisterDTO registerDto)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new RegistrationDTO
                {
                    Success = false,
                    Error = "User with this email already exists",
                    ErrorCode = "CONFLICT"
                };
            }

            var user = _mapper.Map<ApplicationUser>(registerDto);
            var code = GenerateOtp();
            user.EmailVerificationCode = HashOtp(code, _otpSecret);
            user.EmailVerificationCodeExpiration = DateTime.UtcNow.AddMinutes(20);
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new RegistrationDTO
                {
                    Success = false,
                    Error = "Error creating the user"
                };
            }

            string role = registerDto.Role;

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            await _userManager.AddToRoleAsync(user, role);

            _backgroundJobService.Enqueue(()=> _emailSender.SendEmailAsync(user.Email, "Verification Code",
                $"Your verification code is: {code}"));
            // await _emailSender.SendEmailAsync(user.Email, "Verification Code",
                // $"Your verification code is: {code}");
            var Registrationdto = new RegistrationDTO
            {
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                Message = "Registration successful. Verification code sent to your email.",
                Success = true

            };

            return Registrationdto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RegisterAsync failed");
            return new RegistrationDTO
            {
                Success = false,
                Error = $"An error occurred during registration: {ex.Message}"
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
                var failResponse = result.IsLockedOut ? Fail("Account is locked out") :
                                 result.IsNotAllowed ? Fail("Login not allowed") :
                                 Fail("Invalid email or password");
                
                failResponse.EmailConfirmed = user.EmailConfirmed;
                return failResponse;
            }

            var (accessToken, jti, expiry) = await _tokenService.GenerateJwtTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenHash = _tokenService.ComputeSha256Hash(refreshToken);
            var dbToken = new RefreshToken
            {
                TokenHash = refreshTokenHash,
                JwtId = jti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(20),
                CreatedDate = DateTime.UtcNow,
                IsUsed = false,
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(dbToken);
            await _unitOfWork.SaveChangesAsync();

            var authResult = _mapper.Map<AuthResultDTO>(user);
            authResult.RefreshToken = refreshToken;
            authResult.RefreshTokenExpiration = dbToken.ExpiryDate;
            authResult.AccessToken = accessToken;
            authResult.Success = true;
            authResult.Role = roles.FirstOrDefault() ?? "Patient";
            authResult.EmailConfirmed = user.EmailConfirmed;
            return authResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoginAsync failed for email: {Email}", loginDto.Email);
            return new AuthResultDTO
            {
                Success = false,
                Error = $"An error occurred during login: {ex.Message}"
            };
        }
    }

    public async Task<AuthResultDTO> GoogleLoginAsync(GoogleLoginDTO googleLoginDto)
    {
        try
        {
            var audienceList = new[] { _googleWebClientId, _googleIosClientId, _googleAndroidClientId }
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = audienceList.Count > 0 ? audienceList : null
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDto.IdToken, settings);

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FullName = payload.Name ?? payload.Email,
                    EmailConfirmed = true,
                    IsEmailVerified = true
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return Fail("Error creating Google user account");

                string role = "Patient";
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                await _userManager.AddToRoleAsync(user, role);
            }
            else if (!user.EmailConfirmed && payload.EmailVerified)
            {
                user.EmailConfirmed = true;
                user.IsEmailVerified = true;
                await _userManager.UpdateAsync(user);
            }

            var (accessToken, jti, expiry) = await _tokenService.GenerateJwtTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenHash = _tokenService.ComputeSha256Hash(refreshToken);
            var dbToken = new RefreshToken
            {
                TokenHash = refreshTokenHash,
                JwtId = jti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(20),
                CreatedDate = DateTime.UtcNow,
                IsUsed = false,
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(dbToken);
            await _unitOfWork.SaveChangesAsync();

            var authResult = _mapper.Map<AuthResultDTO>(user);
            authResult.RefreshToken = refreshToken;
            authResult.RefreshTokenExpiration = dbToken.ExpiryDate;
            authResult.AccessToken = accessToken;
            authResult.Success = true;
            authResult.Role = "Patient";
            authResult.EmailConfirmed = user.EmailConfirmed;
            return authResult;
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google ID Token");
            return Fail("Invalid Google ID Token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GoogleLoginAsync failed");
            return Fail($"An error occurred during Google login: {ex.Message}");
        }
    }

    public async Task<ConfirmMailDTO> ConfirmEmailCodeAsync(string Email, string ComfirmationCode)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(Email);
            string hashedCode = HashOtp(ComfirmationCode, _otpSecret);
            if (user == null)
            {
                return new ConfirmMailDTO
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = "NOT_FOUND"
                };
            }

            if (user.EmailVerificationCodeExpiration >= DateTime.UtcNow)
            {
                if (user.EmailConfirmed)
                {
                    return new ConfirmMailDTO
                    {
                        Success = true,
                        Message = "Email is already confirmed"
                    };
                }
                else if (hashedCode == user.EmailVerificationCode)
                {
                    user.IsEmailVerified = true;
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    return new ConfirmMailDTO
                    {
                        Success = true,
                        Message = "Email confirmed successfully"
                    };

                }
                else
                {
                    return new ConfirmMailDTO
                    {
                        Success = false,
                        Message = "The confirmation code is not correct",
                        ErrorCode = "INVALID_CODE"
                    };
                }
            }
            else
            {
                return new ConfirmMailDTO
                {
                    Success = false,
                    Message = "The confirmation code has expired",
                    ErrorCode = "EXPIRED_CODE"
                };
            }
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
                .FindAsync(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedDate = DateTime.UtcNow;
                await _refreshTokenRepository.UpdateAsync(token);
            }
            await _unitOfWork.SaveChangesAsync();
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

            var storedTokens = (await _refreshTokenRepository.FindAsync(rt => rt.TokenHash == hashed));
            var storedToken = storedTokens.SingleOrDefault();

            if (storedToken == null)
                return Fail("Invalid refresh token");

            if (storedToken.IsUsed)
            {
                await RevokeAllRefreshTokensForUser(userId);
                return Fail("Refresh token has already been used");
            }

            if (storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow || storedToken.JwtId != jti)
            {
                return Fail("Invalid refresh token");
            }

            storedToken.IsUsed = true;
            storedToken.IsRevoked = true;
            storedToken.RevokedDate = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(storedToken);
            await _unitOfWork.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Fail("User not found");

            var (newAccessToken, newJti, newExpiry) = await _tokenService.GenerateJwtTokenAsync(user);
            var newRefreshTokenPlain = _tokenService.GenerateRefreshToken();
            var newRefreshTokenHash = _tokenService.ComputeSha256Hash(newRefreshTokenPlain);

            var newDbToken = new RefreshToken
            {
                TokenHash = newRefreshTokenHash,
                JwtId = newJti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddMinutes(7),
                CreatedDate = DateTime.UtcNow,
                IsUsed = false,
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(newDbToken);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResultDTO
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenPlain,
                RefreshTokenExpiration = newDbToken.ExpiryDate,
                Message = "Token refreshed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshTokenAsync failed");
            return new AuthResultDTO
            {
                Success = false,
                Error = $"An error occurred during token refresh: {ex.Message}"
            };
        }
    }

    public async Task<MessageResultDTO> ResendVerificationCodeAsync(string Email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return FailMessage("User not found", "NOT_FOUND");

            if (user.EmailConfirmed)
                return FailMessage("Email is already confirmed", "CONFLICT");

            var code = GenerateOtp();
            user.EmailVerificationCode = HashOtp(code, _otpSecret);
            user.EmailVerificationCodeExpiration = DateTime.UtcNow.AddMinutes(20);
            await _userManager.UpdateAsync(user);

            _backgroundJobService.Enqueue(()=> _emailSender.SendEmailAsync(user.Email,
                "Email Verification Code",
                $"Your verification code is: {code}"));

            return new MessageResultDTO
            {
                Success = true,
                Message = "Verification code sent successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResendVerificationCodeAsync failed");
            return new MessageResultDTO
            {
                Success = false,
                Error = "Failed to send verification code"
            };
        }
    }

    public async Task<MessageResultDTO> ForgotPasswordAsync(ForgotPasswordDTO dto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return FailMessage("User not found", "NOT_FOUND");

            if (!user.EmailConfirmed)
                return FailMessage("Email is not confirmed", "UNCONFIRMED");

            var code = GenerateOtp();
            user.PasswordResetCode = HashOtp(code, _otpSecret);
            user.PasswordResetCodeExpiration = DateTime.UtcNow.AddMinutes(15);
            await _userManager.UpdateAsync(user);

            _backgroundJobService.Enqueue(() => _emailSender.SendEmailAsync(
                user.Email,
                "Reset Your Password - Eirene",
                $"Your password reset code is: {code}\n\nThis code will expire in 15 minutes.\nIf you didn't request this, you can ignore this email."));

            return new MessageResultDTO
            {
                Success = true,
                Message = "Password reset code has been sent to your email"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ForgotPasswordAsync failed for email: {Email}", dto.Email);
            return new MessageResultDTO
            {
                Success = false,
                Error = $"An error occurred during forgot password: {ex.Message}"
            };
        }
    }

    public async Task<MessageResultDTO> ResetPasswordAsync(ResetPasswordDTO dto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return FailMessage("User not found", "NOT_FOUND");

            if (user.PasswordResetCodeExpiration < DateTime.UtcNow)
                return FailMessage("The reset code has expired. Please request a new one.", "EXPIRED_CODE");

            var hashedCode = HashOtp(dto.Code, _otpSecret);
            if (hashedCode != user.PasswordResetCode)
                return FailMessage("Invalid reset code", "INVALID_CODE");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return FailMessage($"Failed to reset password: {errors}");
            }

            user.PasswordResetCode = string.Empty;
            user.PasswordResetCodeExpiration = DateTime.MinValue;
            await _userManager.UpdateAsync(user);

            return new MessageResultDTO
            {
                Success = true,
                Message = "Password has been successfully reset"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResetPasswordAsync failed for email: {Email}", dto.Email);
            return new MessageResultDTO
            {
                Success = false,
                Error = $"An error occurred during reset password: {ex.Message}"
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
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RevokeAllRefreshTokensForUser failed for user {UserId}", userId);
        }
    }
    private static string GenerateOtp()
    {
        int code = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return code.ToString("D6");
    }
    private static string HashOtp(string otp, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(otp));
        return Convert.ToBase64String(hash);
    }
    private static AuthResultDTO Fail(string error) =>
        new() { Success = false, Error = error };
    private static MessageResultDTO FailMessage(string error, string errorCode = "") =>
        new() { Success = false, Error = error, ErrorCode = errorCode };
}
