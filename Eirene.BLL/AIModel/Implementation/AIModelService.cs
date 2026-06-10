using System.Text;
using System.Text.Json;
using Eirene.BLL.AIModel.Abstraction;
using Microsoft.Extensions.Options;

namespace Eirene.BLL.AIModel.Implementation;

public class AIModelService : IAIModelService
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly IPythonModelService _pythonModelService;
    private const string MODEL_NAME = "gemini-2.5-flash";

    private const double SUICIDE_WATCH_ALERT_THRESHOLD = 0.4;

    public AIModelService(HttpClient httpClient, IOptions<AISettings> options, IPythonModelService pythonModelService)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _pythonModelService = pythonModelService;
    }

    public async Task<string> AnalyzeUserAnswersAsync(string inputText)
    {
        var url = $"{_settings.GeminiBaseUrl}/models/{MODEL_NAME}:generateContent?key={_settings.GeminiApiKey}";
         
        Dictionary<string, double> modelPrediction =
            await _pythonModelService.PredictMentalHealthIssueAsync(inputText);

        var request = CreateAnalysisRequest(modelPrediction);
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"AIModel API failed ({response.StatusCode}): {error}");
        }

        return await ParseResponseAsync(response);
    }

    private static object CreateAnalysisRequest(Dictionary<string, double> predictionResult)
    {
        return new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = BuildPrompt(predictionResult) }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,      // Low: consistent, structured clinical output
                topP = 0.9,
                topK = 40,
                maxOutputTokens = 4096, // 5 tasks with rationales fits well under this
                responseMimeType = "application/json"
            }
        };
    }

    private static string BuildPrompt(Dictionary<string, double> modelPrediction)
    {
        var sorted = modelPrediction
            .OrderByDescending(kv => kv.Value)
            .ToList();

        string dominantCondition = sorted.First().Key;
        double dominantConfidence = sorted.First().Value;

        string probabilityBreakdown = string.Join("\n", sorted.Select(kv =>
            $"  - {FormatConditionName(kv.Key)}: {kv.Value:P1} confidence"));

        var secondarySignals = sorted.Skip(1)
            .Where(kv => kv.Value >= 0.05 && kv.Key != "control")
            .ToList();

        string secondaryContext = secondarySignals.Any()
            ? $"Secondary signals also present: {string.Join(", ", secondarySignals.Select(kv => $"{FormatConditionName(kv.Key)} ({kv.Value:P1})"))}"
            : "No significant secondary signals detected.";

        bool isControl = dominantCondition.Equals("control", StringComparison.OrdinalIgnoreCase);
        bool isHighConfidence = dominantConfidence >= 0.75;

        bool hasSuicideWatchSignal = modelPrediction.TryGetValue("suicidewatch", out double swProb)
                                     && swProb >= SUICIDE_WATCH_ALERT_THRESHOLD
                                     && dominantCondition != "suicidewatch";

        string safetyOverride = hasSuicideWatchSignal
            ? $"\n SAFETY NOTE: Suicidal Ideation signal detected at {swProb:P1}. Even though it is not dominant, ensure at least one task addresses emotional safety and connection."
            : string.Empty;

        string clinicalContext = isControl
            ? "The patient does not currently exhibit patterns strongly associated with a specific mental health condition. Focus on preventive well-being and resilience building."
            : $"The patient most strongly aligns with {FormatConditionName(dominantCondition)} " +
              $"({dominantConfidence:P1} confidence). " +
              $"{(isHighConfidence ? "This is a high-confidence signal — tailor tasks specifically." : "This is a moderate-confidence signal — keep tasks broadly applicable.")} " +
              secondaryContext +
              safetyOverride;

        string conditionGuidance = GetConditionGuidance(dominantCondition, isControl);

        return $@"You are an experienced, empathetic mental health coach and CBT-trained therapist.

## Clinical Context
{clinicalContext}

## Model Probability Distribution
{probabilityBreakdown}

## Your Task
Based solely on the probability distribution above:
1. Identify at most 3 specific psychological patterns or concerns likely associated with these results.
2. Generate exactly 5 personalized, actionable tasks tailored to the dominant condition.

## Task Design Guidelines
{conditionGuidance}
- Tasks must be concrete and doable within a day or week — no vague advice like ""seek help"".
- Order tasks from easiest to most challenging.
- Each task must include a brief ""why it helps"" rationale.
- If confidence is below 60%, design tasks beneficial across multiple conditions.
- Write directly to the patient: warm, clear, and free of clinical jargon.

## Response Format
Respond ONLY in valid JSON — no markdown, no text outside the JSON:
{{
  ""dominant_condition"": ""{FormatConditionName(dominantCondition)}"",
  ""confidence_level"": ""{(isHighConfidence ? "high" : "moderate")}"",
  ""problems"": [""pattern 1"", ""pattern 2"", ""pattern 3""],
  ""tasks_for_user"": [
    {{""task"": ""task description"", ""rationale"": ""why this helps"", ""difficulty"": ""easy|medium|hard""}},
    {{""task"": ""task description"", ""rationale"": ""why this helps"", ""difficulty"": ""easy|medium|hard""}}
  ]
}}";
    }

    private static string FormatConditionName(string key) => key switch
    {
        "suicidewatch" => "Suicidal Ideation",
        "adhd" => "ADHD",
        "depression" => "Depression",
        "anxiety" => "Anxiety",
        "control" => "No Significant Condition",
        _ => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key)
    };

    private static string GetConditionGuidance(string condition, bool isControl) => condition switch
    {
        "anxiety" => @"- Prioritize grounding techniques (5-4-3-2-1 sensory), box breathing, and worry journaling.
- Include one task to challenge avoidance behavior (gradual exposure).
- Include one task to reduce physiological arousal (progressive muscle relaxation or cold water).",

        "depression" => @"- Prioritize behavioral activation — small, rewarding activities to break inertia.
- Include one social connection task (even low-effort: a text, a walk outside).
- Include one task targeting negative thought patterns (CBT thought record or gratitude journaling).
- Avoid overwhelming tasks; start with the smallest possible win.",

        "suicidewatch" => @"- CRITICAL: Lead with a safety-oriented task (crisis line, trusted contact, safe environment check).
- Include one immediate distress tolerance skill (TIPP: Temperature, Intense exercise, Paced breathing, Paired muscle relaxation).
- Include one reason-for-living reflection exercise.
- Keep all tasks gentle and non-shaming. Prioritize connection and safety above all else.",

        "adhd" => @"- Focus on structure and externalizing systems (timers, written lists, body doubling).
- Include one task using the Pomodoro technique or time-blocking.
- Include one task to reduce environmental distractions.
- Tasks should be short, specific, and immediately actionable.",

        "control" => @"- Focus on preventive well-being: sleep hygiene, movement, and social connection.
- Include one mindfulness or journaling task for emotional awareness.
- Keep tasks light, positive, and habit-forming.",

        _ => @"- Apply general CBT and well-being principles.
- Balance emotional regulation, behavioral activation, and social connection."
    };

    private static async Task<string> ParseResponseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<GeminiResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                   ?? throw new InvalidOperationException("No response text from The AI Model");

        try
        {
            JsonDocument.Parse(text);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"AI Model returned non-JSON content: {ex.Message}\nRaw: {text}");
        }

        return text;
    }
}
