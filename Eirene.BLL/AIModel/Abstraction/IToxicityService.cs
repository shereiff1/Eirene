namespace Eirene.BLL.AIModel.Abstraction;

public interface IToxicityService
{
    Task<ToxicityResult?> AnalyseAsync(string text);
}
