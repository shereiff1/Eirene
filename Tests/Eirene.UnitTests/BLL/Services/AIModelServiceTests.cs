using System.Net;
using System.Text.Json;
using AutoFixture;
using Eirene.BLL.AIModel.Abstraction;
using Eirene.BLL.AIModel.Implementation;
using Eirene.BLL.AIModel;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Eirene.UnitTests.BLL.Services;

public class AIModelServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPythonModelService> _pythonModelServiceMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly AISettings _settings;

    public AIModelServiceTests()
    {
        _fixture = new Fixture();
        _pythonModelServiceMock = new Mock<IPythonModelService>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _settings = new AISettings
        {
            GeminiBaseUrl = "https://generativelanguage.googleapis.com/v1beta",
            GeminiApiKey = "test-api-key"
        };
    }

    private AIModelService CreateSut()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        var options = Options.Create(_settings);
        return new AIModelService(httpClient, options, _pythonModelServiceMock.Object);
    }

    private void SetupHttpMock(HttpStatusCode statusCode, string responseContent)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            });
    }

    private string CreateGeminiResponseJson(string innerJson)
    {
        var responseObj = new GeminiResponse
        {
            Candidates = new List<GeminiCandidate>
            {
                new GeminiCandidate
                {
                    Content = new GeminiContent
                    {
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart { Text = innerJson }
                        }
                    }
                }
            }
        };
        return JsonSerializer.Serialize(responseObj);
    }

    [Fact]
    public async Task AnalyzeUserAnswersAsync_ValidPredictionAndValidGeminiResponse_ReturnsExpectedJson()
    {
        // Arrange
        var inputText = "I feel very sad and have no energy.";
        var predictions = new Dictionary<string, double>
        {
            { "depression", 0.8 },
            { "control", 0.2 }
        };
        
        _pythonModelServiceMock.Setup(p => p.PredictMentalHealthIssueAsync(inputText))
            .ReturnsAsync(predictions);

        var expectedInnerJson = "{\"dominant_condition\": \"Depression\", \"confidence_level\": \"high\", \"problems\": [], \"tasks_for_user\": []}";
        SetupHttpMock(HttpStatusCode.OK, CreateGeminiResponseJson(expectedInnerJson));

        var sut = CreateSut();

        // Act
        var result = await sut.AnalyzeUserAnswersAsync(inputText);

        // Assert
        result.Should().Be(expectedInnerJson);
    }

    [Fact]
    public async Task AnalyzeUserAnswersAsync_SuicideWatchAlertThresholdTriggered_IncludesSafetyNoteInPrompt()
    {
        // Arrange
        var inputText = "Distressed statement.";
        var predictions = new Dictionary<string, double>
        {
            { "depression", 0.5 },
            { "suicidewatch", 0.45 }, // Trigger threshold >= 0.4
            { "control", 0.05 }
        };

        _pythonModelServiceMock.Setup(p => p.PredictMentalHealthIssueAsync(inputText))
            .ReturnsAsync(predictions);

        string? capturedPrompt = null;
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                capturedPrompt = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CreateGeminiResponseJson("{}"))
            });

        var sut = CreateSut();

        // Act
        await sut.AnalyzeUserAnswersAsync(inputText);

        // Assert
        capturedPrompt.Should().NotBeNull();
        capturedPrompt.Should().Contain("SAFETY NOTE: Suicidal Ideation signal detected");
    }

    [Fact]
    public async Task AnalyzeUserAnswersAsync_GeminiApiReturnsNonSuccessStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        var inputText = "Testing HTTP error.";
        var predictions = new Dictionary<string, double> { { "control", 1.0 } };

        _pythonModelServiceMock.Setup(p => p.PredictMentalHealthIssueAsync(inputText))
            .ReturnsAsync(predictions);

        SetupHttpMock(HttpStatusCode.InternalServerError, "Error details from API");

        var sut = CreateSut();

        // Act
        Func<Task> act = async () => await sut.AnalyzeUserAnswersAsync(inputText);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*AIModel API failed (InternalServerError): Error details from API*");
    }

    [Fact]
    public async Task AnalyzeUserAnswersAsync_GeminiApiReturnsNonJsonText_ThrowsInvalidOperationException()
    {
        // Arrange
        var inputText = "Testing non-json return.";
        var predictions = new Dictionary<string, double> { { "control", 1.0 } };

        _pythonModelServiceMock.Setup(p => p.PredictMentalHealthIssueAsync(inputText))
            .ReturnsAsync(predictions);

        SetupHttpMock(HttpStatusCode.OK, CreateGeminiResponseJson("This is not valid JSON."));

        var sut = CreateSut();

        // Act
        Func<Task> act = async () => await sut.AnalyzeUserAnswersAsync(inputText);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*AI Model returned non-JSON content*");
    }

    [Fact]
    public async Task AnalyzeUserAnswersAsync_PredictionConditionFormatting_UsesCorrectConditionName()
    {
        // Arrange
        var inputText = "Testing formatting.";
        var predictions = new Dictionary<string, double>
        {
            { "adhd", 0.6 },
            { "suicidewatch", 0.1 },
            { "control", 0.3 }
        };

        _pythonModelServiceMock.Setup(p => p.PredictMentalHealthIssueAsync(inputText))
            .ReturnsAsync(predictions);

        string? capturedPrompt = null;
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                capturedPrompt = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CreateGeminiResponseJson("{}"))
            });

        var sut = CreateSut();

        // Act
        await sut.AnalyzeUserAnswersAsync(inputText);

        // Assert
        capturedPrompt.Should().NotBeNull();
        capturedPrompt.Should().Contain("ADHD");
        capturedPrompt.Should().Contain("No Significant Condition");
    }

    [Fact]
    public async Task AnalyzeUserAnswersAsync_LowConfidenceSignal_UsesModerateConfidenceGuidance()
    {
        // Arrange
        var inputText = "Moderate confidence test.";
        var predictions = new Dictionary<string, double>
        {
            { "anxiety", 0.65 }, // < 0.75 threshold
            { "control", 0.35 }
        };

        _pythonModelServiceMock.Setup(p => p.PredictMentalHealthIssueAsync(inputText))
            .ReturnsAsync(predictions);

        string? capturedPrompt = null;
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                capturedPrompt = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CreateGeminiResponseJson("{}"))
            });

        var sut = CreateSut();

        // Act
        await sut.AnalyzeUserAnswersAsync(inputText);

        // Assert
        capturedPrompt.Should().NotBeNull();
        capturedPrompt.Should().Contain("This is a moderate-confidence signal");
    }
}
