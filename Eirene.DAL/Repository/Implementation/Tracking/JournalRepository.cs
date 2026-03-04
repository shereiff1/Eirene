using Eirene.DAL.Database;
using Eirene.DAL.Entities.Tracking;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Tracking;
using Microsoft.EntityFrameworkCore;


namespace Eirene.DAL.Repository.Implementation.Tracking;

public class JournalRepository : GenericRepository<Journal>, IJournalRepository
{
    public JournalRepository(EireneDBContext context) : base(context)
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
