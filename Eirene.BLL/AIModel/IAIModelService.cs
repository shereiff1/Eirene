
namespace BLL.AIModel;

public interface IAIModelService
{
    Task<string> AnalyzeUserAnswersAsync(string questionsAndAnswers);
}
