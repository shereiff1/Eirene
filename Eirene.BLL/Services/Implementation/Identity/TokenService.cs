using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.DAL.Entities.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System;
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
        _key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret is missing")));
    }

    public async Task<(string Token, string Jti, DateTime Expiry)> GenerateJwtTokenAsync(ApplicationUser user)
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

    public string GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal?.FindFirst(claimType)?.Value;
    }

    public string GetJtiFromPrincipal(ClaimsPrincipal principal)
    {
        return GetClaimValue(principal, JwtRegisteredClaimNames.Jti);
    }

    public string GetUserIdFromPrincipal(ClaimsPrincipal principal)
    {
        return GetClaimValue(principal, ClaimTypes.NameIdentifier);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string ComputeSha256Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private async Task<List<Claim>> BuildClaimsAsync(ApplicationUser user, string jti)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

        // Add role claims
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

    private double GetTokenExpirationMinutes()
    {
        if (double.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out var minutes))
        {
            return minutes;
        }
        return 15;
    }
}