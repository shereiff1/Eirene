namespace Eirene.DAL.Repository.Abstraction
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}