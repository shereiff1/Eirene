using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Core;

internal class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(EireneDBContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<int> RevokeActiveTokensForUserAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.IsRevoked, true)
                .SetProperty(rt => rt.RevokedDate, DateTime.UtcNow));
    }

    /// <inheritdoc/>
    public async Task<int> RevokeAllTokensForUserAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && (!rt.IsRevoked || !rt.IsUsed))
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.IsRevoked, true)
                .SetProperty(rt => rt.RevokedDate, DateTime.UtcNow));
    }
}
