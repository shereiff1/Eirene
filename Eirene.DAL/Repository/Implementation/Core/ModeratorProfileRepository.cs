using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Core;

internal class ModeratorProfileRepository :  GenericRepository<ModeratorProfile>, IModeratorProfileRepository
{
    public ModeratorProfileRepository(EireneDBContext context) : base(context)
    {
    }
}
