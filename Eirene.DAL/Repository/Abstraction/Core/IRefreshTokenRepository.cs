using Eirene.DAL.Entities.Core;

namespace Eirene.DAL.Repository.Abstraction.Core;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<int> RevokeActiveTokensForUserAsync(string userId);
    Task<int> RevokeAllTokensForUserAsync(string userId);
}