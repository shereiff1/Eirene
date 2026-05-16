using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;

namespace Eirene.DAL.Repository.Implementation.Core;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(EireneDBContext context) : base(context)
    {
    }
}
