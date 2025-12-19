using DAL.Database;
using DAL.Entities.Treatment;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Treatment;
using Microsoft.EntityFrameworkCore;
namespace DAL.Repository.Implementation.Treatment;

public class QuestionAnswerRepository : GenericRepository<QuestionAnswer>, IQuestionAnswerRepository
{
    public QuestionAnswerRepository(EireneDBContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
    public async Task<IEnumerable<QuestionAnswer>> GetAnswersByUserIdAsync(string userId)
    {
        return await _context.QuestionAnswers
            .Where(qa => qa.PatientId == userId)
            .Include(qa => qa.Question)
            .ToListAsync();
    }
}
