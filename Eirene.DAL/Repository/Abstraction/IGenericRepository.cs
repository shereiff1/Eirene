using System.Linq.Expressions;

namespace Eirene.DAL.Repository.Abstraction
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();

        Task<(List<T> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);

        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task<(List<T> Items, int TotalCount)> FindPagedAsync(Expression<Func<T, bool>> predicate, int page, int pageSize);

        Task<T?> GetByIdAsync(object id);

        Task<T> AddAsync(T entity);

        Task<bool> UpdateAsync(T entity);

        Task<bool> DeleteAsync(T entity);
    }
}