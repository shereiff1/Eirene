using System.Text.Json.Serialization;


namespace BLL.AIModel;

public class Content
{
    [JsonPropertyName("parts")]
    public List<Part>? Parts { get; set; }
}