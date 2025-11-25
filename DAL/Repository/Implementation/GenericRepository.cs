using DAL.Repository.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
    }
}
