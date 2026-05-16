

using System.Text.Json.Serialization;

namespace Eirene.BLL.Models.Model_Result;

public class AITaskResponse
{
    [JsonPropertyName("dominant_condition")]
    public string DominantCondition { get; set; } = string.Empty;

    [JsonPropertyName("confidence_level")]
    public string ConfidenceLevel { get; set; } = string.Empty;

    [JsonPropertyName("problems")]
    public List<string> Problems { get; set; } = new List<string>();

    [JsonPropertyName("tasks_for_user")]
    public List<TaskItem> TasksForUser { get; set; } = new List<TaskItem>();
}
