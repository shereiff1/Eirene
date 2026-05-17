using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.DAL.Entities.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Security.Claims;


namespace Eirene.BLL.Services.Implementation.Identity;

public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly SymmetricSecurityKey _key;

    public TokenService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<TokenService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
         
        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Secret is missing from configuration. " +
                "Add a sufficiently long random string to appsettings.json under JwtSettings:Secret.");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public async Task<(string Token, string Jti, DateTime Expiry)> GenerateJwtTokenAsync(ApplicationUser user)
    { 
        try
        {
            var jti = Guid.NewGuid().ToString();
            var claims = await BuildClaimsAsync(user, jti);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (
                tokenHandler.WriteToken(token),
                jti,
                token.ValidTo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate JWT token for user {UserId}", user.Id);
            throw;
        }
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var validationParameters = GetTokenValidationParameters(validateLifetime: false);
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Failed to validate token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return null;
        }
    }
     
    public string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal?.FindFirst(claimType)?.Value;
    }

    public string? GetJtiFromPrincipal(ClaimsPrincipal principal)
    {
        return GetClaimValue(principal, JwtRegisteredClaimNames.Jti);
    }

    public string? GetUserIdFromPrincipal(ClaimsPrincipal principal)
    {
        return GetClaimValue(principal, ClaimTypes.NameIdentifier);
    }

    public string GenerateRefreshToken()
    { 
        try
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate refresh token");
            throw;
        }
    }

    public string ComputeSha256Hash(string input)
    { 
        try
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute SHA256 hash");
            throw;
        }
    }

    private async Task<List<Claim>> BuildClaimsAsync(ApplicationUser user, string jti)
    { 
        if (string.IsNullOrEmpty(user.UserName))
        {
            _logger.LogWarning("User {UserId} has no username — token will have empty Sub claim", user.Id);
            throw new InvalidOperationException($"Cannot generate token: user {user.Id} has no username.");
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogWarning("User {UserId} has no email — token will have empty Email claim", user.Id);
            throw new InvalidOperationException($"Cannot generate token: user {user.Id} has no email.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private TokenValidationParameters GetTokenValidationParameters(bool validateLifetime = true)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateIssuer = true,
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["JwtSettings:Audience"],
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero
        };
    }
     
    private int GetTokenExpirationMinutes()
    {
        if (int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out var minutes))
        {
            return minutes;
        }

        _logger.LogWarning(
            "JwtSettings:AccessTokenExpirationMinutes is missing or invalid. Defaulting to 15 minutes.");

        return 15;
    }
}