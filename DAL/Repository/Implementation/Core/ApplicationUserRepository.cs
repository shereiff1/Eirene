using DAL.Entities.Core;
using DAL.Repository.Abstraction.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Core
{
    internal class ApplicationUserRepository: GenericRepository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(DbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}
