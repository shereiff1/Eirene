
using System.Text.Json.Serialization;

namespace BLL.AIModel;

public class AIModelResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate>? Candidates { get; set; }
}
