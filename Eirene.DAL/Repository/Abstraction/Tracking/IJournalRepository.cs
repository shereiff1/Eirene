using DAL.Entities.Tracking;


namespace DAL.Repository.Abstraction.Tracking;

public interface IJournalRepository : IGenericRepository<Journal>
{
    Task<Journal?> GetTodayJournalAsync(string userId, DateTime date);
    Task<List<Journal>> GetAllForUserAsync(string userId);
}