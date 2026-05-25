using System.Text.Json.Serialization;

namespace Eirene.BLL.AIModel;

/*
 * Gemini API Response...
 * 
 GeminiResponse         ← The entire HTTP response from Gemini
  └── Candidates[]      ← List of possible answers (usually just 1)
        └── Content     ← The actual message from the AI
              └── Parts[]  ← Pieces of that message
                    └── Text  ← The actual string you want to read
 */
public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
}

public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
}

public class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart>? Parts { get; set; }
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
