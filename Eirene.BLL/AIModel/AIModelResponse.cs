
using System.Text.Json.Serialization;

namespace Eirene.BLL.AIModel;

public class AIModelResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate>? Candidates { get; set; }
}
