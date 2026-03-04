using System.Security.Claims;
using Eirene.DAL.Entities.Core;

namespace Eirene.BLL.Services.Abstraction.Identity
{
    public interface ITokenService
    {
        Task<(string Token, string Jti, DateTime Expiry)> GenerateJwtTokenAsync(ApplicationUser user);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
        string GetClaimValue(ClaimsPrincipal principal, string claimType);
        string GetJtiFromPrincipal(ClaimsPrincipal principal);
        string GetUserIdFromPrincipal(ClaimsPrincipal principal);
        string GenerateRefreshToken();
        string ComputeSha256Hash(string input);
    }
}