using DAL.Entities.Treatment;

namespace BLL.Services.Abstraction.Treatment;

public interface IQuestionAnswerServices
{
    Task<(bool IsSuccess, IEnumerable<QuestionAnswer> Answers)> GetAnswersForUserAsync(string userId);
    Task<(bool IsSuccess, QuestionAnswer Answer)> AddAnswerAsync(string userId, int questionId, string answer);

    Task<(bool IsSuccess, IEnumerable<QuestionAnswer> Answers)> AddMultipleAnswersAsync(string userId,
        List<(int QuestionId, string Answer)> answers);
}