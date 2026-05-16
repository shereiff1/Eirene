// PythonModelService.cs
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Eirene.BLL.AIModel;

public class PythonModelService : IPythonModelService
{
    private readonly HttpClient _httpClient;
    private readonly PythonModelSettings _settings;

    public PythonModelService(HttpClient httpClient, IOptions<PythonModelSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<Dictionary<string, double>> PredictMentalHealthIssueAsync(string text)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_settings.BaseUrl}/predict",
            new { text }
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Python model service failed ({response.StatusCode}): {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, double>>();
        return result ?? throw new InvalidOperationException("Empty response from Python model service");
    }
}