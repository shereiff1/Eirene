using System.Text.Json.Serialization;

namespace Eirene.BLL.AIModel;

public class ModelResultDTO
{
    [JsonPropertyName("analysis")]
    public string Analysis { get; set; } = null!;

    [JsonPropertyName("answersCount")]
    public int AnswersCount { get; set; }
}