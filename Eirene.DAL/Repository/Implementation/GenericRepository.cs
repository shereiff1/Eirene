using Eirene.DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Eirene.DAL.Database;

namespace Eirene.DAL.Repository.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly EireneDBContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(EireneDBContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return true;
        }
    }
}
