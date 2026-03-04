
using System.Linq.Expressions;


namespace Eirene.DAL.Repository.Abstraction
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();

        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task<T?> GetByIdAsync(object id);

        Task<T?> AddAsync(T entity);

        Task<bool> UpdateAsync(T entity);

        Task<bool> DeleteAsync(T entity);
    }
}