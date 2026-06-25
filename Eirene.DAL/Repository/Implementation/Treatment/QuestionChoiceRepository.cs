using Eirene.DAL.Database;
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Treatment;

internal class QuestionChoiceRepository : GenericRepository<QuestionChoice>, IQuestionChoiceRepository
{
    public QuestionChoiceRepository(EireneDBContext context) : base(context)
    {
    }

    public async Task<List<QuestionChoice>> GetChoicesByQuestionIdAsync(Guid questionId)
    {
        return await _context.Set<QuestionChoice>()
            .Where(c => c.QuestionId == questionId)
            .ToListAsync();
    }
}
