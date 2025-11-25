using DAL.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstraction.Core
{
    public interface IUserRepository : IGenericRepository<User>
    {
    }
}
