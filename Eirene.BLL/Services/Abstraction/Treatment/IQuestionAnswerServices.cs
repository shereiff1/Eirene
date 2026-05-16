using Eirene.DAL.Entities.Treatment;

namespace Eirene.BLL.Services.Abstraction.Treatment;

public interface IQuestionAnswerServices
{
    Task<(bool IsSuccess, IEnumerable<QuestionAnswer> Answers)> GetAnswersForUserAsync(string userId);
    Task<(bool IsSuccess, QuestionAnswer Answer)> AddAnswerAsync(string userId, Guid questionId, string answer);

    Task<(bool IsSuccess, IEnumerable<QuestionAnswer> Answers)> AddMultipleAnswersAsync(string userId,
        List<(Guid QuestionId, string Answer)> answers);
}