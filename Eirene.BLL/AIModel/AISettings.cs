namespace Eirene.BLL.AIModel;

/// <summary>
/// Unified settings for both the Gemini API and the Python ML model service.
/// Bound from the "AIModel" configuration section.
/// </summary>
public class AISettings
{
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiBaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public string PythonBaseUrl { get; set; } = string.Empty;
    public string ChatbotBaseUrl { get; set; } = string.Empty;
}
