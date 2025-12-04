using DAL.Entities.Community;
using DAL.Repository.Abstraction.Community;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Database;
using DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Community
{
    internal class CommunityCommentRepository: GenericRepository<CommunityComment>, ICommunityCommentRepository
    {
        public CommunityCommentRepository(EireneDBContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}
