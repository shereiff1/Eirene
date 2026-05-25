using Eirene.DAL.Database;
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Treatment;

internal class QuestionAnswerRepository : GenericRepository<QuestionAnswer>, IQuestionAnswerRepository
{
    public QuestionAnswerRepository(EireneDBContext context) : base(context)
    {
    }
    public async Task<List<QuestionAnswer>> GetAnswersByUserIdAsync(string userId)
    {
        return await _context.Set<QuestionAnswer>()
            .Where(qa => qa.PatientId == userId)
            .Include(qa => qa.Question)
            .ToListAsync();
    }
}
