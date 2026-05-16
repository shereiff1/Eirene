using System.Text.Json.Serialization;

namespace Eirene.BLL.AIModel;

public class AnalysisDTO
{
    [JsonPropertyName("problems")] public List<string> Problems { get; set; } = new();

    [JsonPropertyName("tasks_for_user")] public List<string> Tasks_For_User { get; set; } = new();
}