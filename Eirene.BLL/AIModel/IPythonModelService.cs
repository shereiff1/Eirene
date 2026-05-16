namespace Eirene.BLL.AIModel;

public interface IPythonModelService
{
    Task<Dictionary<string, double>> PredictMentalHealthIssueAsync(string text);
}
