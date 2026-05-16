
using System.Text.Json.Serialization;


namespace Eirene.BLL.AIModel;

public class Candidate
{
    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }
}

