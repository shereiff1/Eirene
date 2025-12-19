using DAL.Database;
using DAL.Entities.Core;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Core;

namespace DAL.Repository.Implementation.Core;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(EireneDBContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
}