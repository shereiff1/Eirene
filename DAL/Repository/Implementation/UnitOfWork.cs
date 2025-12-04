using DAL.Repository.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Database;

namespace DAL.Repository.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EireneDBContext _context;

        public UnitOfWork(EireneDBContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}