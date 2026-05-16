

using Eirene.DAL.Entities.Treatment;

namespace Eirene.DAL.Repository.Abstraction.Treatment;

public interface IQuestionAnswerRepository : IGenericRepository<QuestionAnswer>
{
    Task<IEnumerable<QuestionAnswer>> GetAnswersByUserIdAsync(string userId);
}
