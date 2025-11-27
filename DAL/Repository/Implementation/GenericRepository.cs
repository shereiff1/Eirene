using DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Repository.Implementation
{
    internal class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            var affected = await _unitOfWork.SaveChangesAsync();
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            var affected = await _unitOfWork.SaveChangesAsync();
            return affected > 0;
        }
    }
}