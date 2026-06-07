using System.Text.Json.Serialization;

namespace Eirene.BLL.AIModel;
public class ToxicityResult
{
    [JsonPropertyName("scores")]
    public ToxicityScores Scores { get; set; } = new();

    [JsonPropertyName("violation_score")]
    public double ViolationScore { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = "none";
}

public class ToxicityScores
{
    [JsonPropertyName("toxicity")]
    public double Toxicity { get; set; }

    [JsonPropertyName("severe_toxicity")]
    public double SevereToxicity { get; set; }

    [JsonPropertyName("obscene")]
    public double Obscene { get; set; }

    [JsonPropertyName("threat")]
    public double Threat { get; set; }

    [JsonPropertyName("insult")]
    public double Insult { get; set; }

    [JsonPropertyName("identity_attack")]
    public double IdentityAttack { get; set; }
}
