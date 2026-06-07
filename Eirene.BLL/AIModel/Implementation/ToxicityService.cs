using System.Net.Http.Json;
using Eirene.BLL.AIModel.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eirene.BLL.AIModel.Implementation;

public class ToxicityService : IToxicityService
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly ILogger<ToxicityService> _logger;

    public ToxicityService(HttpClient httpClient, IOptions<AISettings> settings, ILogger<ToxicityService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ToxicityResult?> AnalyseAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.PythonBaseUrl}/predict/toxicity",
                new { text });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Toxicity service returned {StatusCode}: {Error}",
                    response.StatusCode, error);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ToxicityResult>();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach the toxicity prediction service");
            return null;
        }
    }
}
