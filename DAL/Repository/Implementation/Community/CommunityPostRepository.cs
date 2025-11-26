using DAL.Entities.Community;
using DAL.Entities.Core;
using DAL.Repository.Abstraction.Community;
using DAL.Repository.Abstraction.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Implementation.Community
{
    internal class CommunityPostRepository: GenericRepository<CommunityPost>, ICommunityPostRepository
    {
    }
}
