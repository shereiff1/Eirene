namespace Eirene.BLL.AIModel.Abstraction;

public interface IAIModelService
{
    Task<string> AnalyzeUserAnswersAsync(string inputText);
}
