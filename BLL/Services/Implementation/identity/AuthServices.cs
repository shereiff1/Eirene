using AutoMapper;
using BLL.Models.Identity;
using BLL.Services.Abstraction.Identity;
using DAL.Entities.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginDTO = BLL.Models.Identity.LoginDTO;

namespace BLL.Services.Implementation.identity
{
    public class AuthServices : IAuthServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public AuthServices(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IMapper mapper,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _mapper = mapper;
            _emailSender = emailSender;
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

                user.EmailVerificationCode = code;
                user.EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(10);

                await _userManager.UpdateAsync(user);
                await _emailSender.SendEmailAsync(user.Email, "Verification Code",
                    $"Your verification code is: {code}");

                var authResult = _mapper.Map<AuthResultDTO>(user);
                authResult.Success = true;
                authResult.Message = "Registration successful. Verification code sent to your email.";

                return authResult;
            }
            catch (Exception ex)
            {
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
                {
                    return new AuthResultDTO
                    {
                        Success = false,
                        Errors = new[] { "Invalid email or password" }
                    };
                }

                if (!user.EmailConfirmed)
                {
                    return new AuthResultDTO
                    {
                        Success = false,
                        Errors = new[] { "Email not confirmed. Please confirm your email first." }
                    };
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    loginDto.Password,
                    loginDto.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    var token = await GenerateJwtToken(user);
                    var roles = await _userManager.GetRolesAsync(user);

                    var authResult = _mapper.Map<AuthResultDTO>(user);
                    authResult.Success = true;
                    authResult.Token = token;
                    authResult.Role = roles.FirstOrDefault();
                    authResult.Message = "Login successful";

                    return authResult;
                }

                if (result.IsLockedOut)
                {
                    return new AuthResultDTO
                    {
                        Success = false,
                        Errors = new[] { "Account locked due to multiple failed login attempts" }
                    };
                }

                return new AuthResultDTO
                {
                    Success = false,
                    Errors = new[] { "Invalid email or password" }
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDTO
                {
                    Success = false,
                    Errors = new[] { $"An error occurred during login: {ex.Message}" }
                };
            }
        }

        public async Task<AuthResultDTO> ConfirmEmailCodeAsync(ConfirmEmailCode dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return new AuthResultDTO
                    {
                        Success = false,
                        Errors = new[] { "User not found" }
                    };
                }

                if (user.EmailVerificationCode != dto.Code ||
                    user.EmailVerificationExpiry < DateTime.UtcNow)
                {
                    return new AuthResultDTO
                    {
                        Success = false,
                        Errors = new[] { "Invalid or expired verification code" }
                    };
                }

                user.EmailConfirmed = true;
                user.EmailVerificationCode = null;
                user.EmailVerificationExpiry = null;

                await _userManager.UpdateAsync(user);

                return new AuthResultDTO
                {
                    Success = true,
                    Message = "Email confirmed successfully"
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDTO
                {
                    Success = false,
                    Errors = new[] { $"An error occurred during email confirmation: {ex.Message}" }
                };
            }
        }

        public async Task LogoutAsync(string userId)
        {
            await _signInManager.SignOutAsync();
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                   new Claim(ClaimTypes.NameIdentifier, user.Id),
                   new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                   new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var secret = _configuration["JwtSettings:Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddDays(
                double.Parse(_configuration["JwtSettings:ExpirationInDays"])
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}