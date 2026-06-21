namespace Eirene.BLL.AIModel;

public class AISettings
{
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiBaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public string PythonBaseUrl { get; set; } = string.Empty;
    public string ChatbotBaseUrl { get; set; } = string.Empty;
}
