using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Eirene.DAL.Database;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Treatment;

internal class QuestionRepository : GenericRepository<Question>, IQuestionRepository
{
    public QuestionRepository(EireneDBContext context) : base(context)
    {
    }

    public async Task<Question?> GetByIdWithChoicesAsync(Guid id)
    {
        return await _context.Set<Question>()
            .Include(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<Question>> GetAllWithChoicesAsync()
    {
        return await _context.Set<Question>()
            .Include(q => q.Choices)
            .ToListAsync();
    }
}
