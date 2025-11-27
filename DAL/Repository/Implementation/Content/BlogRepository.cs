using DAL.Entities.Content;
using DAL.Entities.Core;
using DAL.Repository.Abstraction.Content;
using DAL.Repository.Abstraction.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Content
{
    internal class BlogRepository: GenericRepository<Blog>, IBlogRepository
    {
        public BlogRepository(DbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}
