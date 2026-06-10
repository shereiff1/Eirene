namespace Eirene.BLL.AIModel.Abstraction;

public interface IPythonModelService
{
    Task<Dictionary<string, double>> PredictMentalHealthIssueAsync(string text);
}
