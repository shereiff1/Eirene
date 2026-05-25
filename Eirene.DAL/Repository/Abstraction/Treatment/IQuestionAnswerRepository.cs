using Eirene.DAL.Entities.Treatment;

namespace Eirene.DAL.Repository.Abstraction.Treatment;

public interface IQuestionAnswerRepository : IGenericRepository<QuestionAnswer>
{
    Task<List<QuestionAnswer>> GetAnswersByUserIdAsync(string userId);
}
