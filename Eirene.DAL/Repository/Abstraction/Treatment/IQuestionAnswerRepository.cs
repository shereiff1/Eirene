

using DAL.Entities.Treatment;

namespace DAL.Repository.Abstraction.Treatment;

public interface IQuestionAnswerRepository : IGenericRepository<QuestionAnswer>
{
    Task<IEnumerable<QuestionAnswer>> GetAnswersByUserIdAsync(string userId);
}
