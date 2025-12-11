using DAL.Database;
using DAL.Entities.Tracking;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Tracking;
using Microsoft.EntityFrameworkCore;


namespace DAL.Repository.Implementation.Tracking;

public class JournalRepository : GenericRepository<Journal>, IJournalRepository
{
    public JournalRepository(EireneDBContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }

    public Task<Journal?> GetTodayJournalAsync(string userId, DateTime date)
    {
        return _context.Journals
            .Where(j => j.PatientId == userId && j.CreatedAt.Date == date.Date)
            .FirstOrDefaultAsync();
    }

    public Task<List<Journal>> GetAllForUserAsync(string userId)
    {
        return _context.Journals.Where(j => j.PatientId == userId).ToListAsync();
    }
}