
using System.Text.Json.Serialization;

namespace Eirene.BLL.Models.Model_Result;

public class TaskItem
{
    [JsonPropertyName("task")]
    public string Task { get; set; } = string.Empty;

    [JsonPropertyName("rationale")]
    public string Rationale { get; set; } = string.Empty;

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;
}
