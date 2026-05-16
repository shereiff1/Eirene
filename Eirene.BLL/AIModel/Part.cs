using System.Text.Json.Serialization;


namespace Eirene.BLL.AIModel;

public class Part
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}