using Eirene.DAL.Entities.Treatment;

namespace Eirene.DAL.Repository.Abstraction.Treatment;

public interface IQuestionChoiceRepository : IGenericRepository<QuestionChoice>
{
    Task<List<QuestionChoice>> GetChoicesByQuestionIdAsync(Guid questionId);
}
