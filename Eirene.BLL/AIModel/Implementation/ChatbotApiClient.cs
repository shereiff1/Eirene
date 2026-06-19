using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Eirene.BLL.AIModel.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eirene.BLL.AIModel.Implementation;

public class ChatbotApiClient : IChatbotApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly ILogger<ChatbotApiClient> _logger;

    public ChatbotApiClient(HttpClient httpClient, IOptions<AISettings> settings, ILogger<ChatbotApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string?> ChatAsync(string message, List<ChatHistoryEntry> history)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        try
        {
            var requestBody = new ChatbotRequest
            {
                Message = message,
                History = history.Select(h => new ChatbotHistoryItem
                {
                    Role = h.Role,
                    Content = h.Content
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.ChatbotBaseUrl}/chat",
                requestBody);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Chatbot service returned 429 Too Many Requests — queue is full");
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogWarning("Chatbot service returned 503 Service Unavailable — request timed out in queue");
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Chatbot service returned {StatusCode}: {Error}",
                    response.StatusCode, error);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ChatbotApiResponse>();
            return result?.Response;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Chatbot service request timed out");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach the chatbot service");
            return null;
        }
    }

    private class ChatbotRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("history")]
        public List<ChatbotHistoryItem> History { get; set; } = new();
    }

    private class ChatbotHistoryItem
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class ChatbotApiResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }
    }
}
