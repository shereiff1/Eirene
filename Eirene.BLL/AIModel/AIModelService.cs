using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Eirene.BLL.AIModel;

public class AIModelService : IAIModelService
{
    private readonly HttpClient _httpClient;
    private readonly AIModelSettings _settings;
    private const string MODEL_NAME = "gemini-2.5-flash";

    public AIModelService(HttpClient httpClient, IOptions<AIModelSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<string> AnalyzeUserAnswersAsync(string questionsAndAnswers)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{MODEL_NAME}:generateContent?key={_settings.ApiKey}";

        var request = CreateAnalysisRequest(questionsAndAnswers);
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

    private static object CreateAnalysisRequest(string questionsAndAnswers)
    {
        return new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = BuildPrompt(questionsAndAnswers) }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 1.0,
                topP = 0.95,
                topK = 64,
                maxOutputTokens = 65536,
                responseMimeType = "application/json"
            }
        };
    }

    private static string BuildPrompt(string questionsAndAnswers)
    {
        return $@"You are a physiatrist. Analyze the following questions and patient answers to notice responses and patterns that indicates mental health issues , respond with
[1: if the patient has a mental health issues only (yes or no)]
[2: if the patient has a mental health issue how severe it is (a percentage)]
[3: if the patient has a mental health issue name the exact mental disorder]
[4: if the patient has a mental health issues generate at most 5 tasks related to his exact disorder to help him get better , if the patient does not have mental health issues generate at most 5 general tasks to improve his mental health quality]
.
reasoning to solve this problem:
1-analyze the patient responses ,look visible or deep patterns or indicators of mental disorders.
2-search what is this pattern are symptoms of.
3-identify severity.
4-look for cognitive behavioral therapy tasks that helps overcoming this exact mental disorder.

Respond ONLY in valid JSON format:
{{
  ""problems"": [""problem 1"", ""problem 2""],
  ""tasks_for_user"": [""task 1"", ""task 2""]
}}

Questions and User Answers:
{questionsAndAnswers}";
    }

    private static async Task<string> ParseResponseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AIModelResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
               ?? throw new InvalidOperationException("No response text from The AI Model");
    }
}
/*
 
 return $@"You are a physiatrist. Analyze the following questions and patient answers to notice responses and patterns that indicates mental health issues , respond with
[1: if the patient has a mental health issues only (yes or no)]
[2: if the patient has a mental health issue how severe it is (a percentage)]
[3: if the patient has a mental health issue name the exact mental disorder]
[4: if the patient has a mental health issues generate at most 5 tasks related to his exact disorder to help him get better , if the patient does not have mental health issues generate at most 5 general tasks to improve his mental health quality]
.
reasoning to solve this problem:
1-analyze the patient responses ,look visible or deep patterns or indicators of mental disorders.
2-search what is this pattern are symptoms of.
3-identify severity.
4-look for cognitive behavioral therapy tasks that helps overcoming this exact mental disorder.

Respond ONLY in valid JSON format:
{{
  ""problems"": [""problem 1"", ""problem 2""],
  ""tasks_for_user"": [""task 1"", ""task 2""]
}}

Questions and User Answers:
{questionsAndAnswers}";
    }



$@"You are a diagnostic assistant. Analyze the following questions and answers.

Tasks:
1. Evaluate whether the user shows signs of any problems
2. Explain clearly what problems you detect (if any)
3. Recommend actionable tasks for the user

Respond ONLY in valid JSON format:
{{
  ""problems"": [""problem 1"", ""problem 2""],
  ""tasks_for_user"": [""task 1"", ""task 2""]
}}

Questions and User Answers:
{questionsAndAnswers}";
 */