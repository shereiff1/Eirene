using Eirene.BLL.AIModel.Abstraction;
using Eirene.BLL.Models.Model_Result;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Abstraction.Treatment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Eirene.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DiagnosisController : ControllerBase
{
    private readonly IAIModelService _modelService;
    private readonly ILogger<DiagnosisController> _logger;
    private readonly IQuestionAnswerServices _questionAnswerServices;
    private readonly IPatientTaskServices _taskServices;
    private readonly IPatientServices _patientServices;
    private readonly IUserContext _userContext;

    public DiagnosisController(
        IAIModelService modelService,
        ILogger<DiagnosisController> logger,
        IQuestionAnswerServices questionAnswerServices,
        IPatientTaskServices taskServices,
        IPatientServices patientServices,
        IUserContext userContext)
    {
        _modelService = modelService;
        _logger = logger;
        _questionAnswerServices = questionAnswerServices;
        _taskServices = taskServices;
        _patientServices = patientServices;
        _userContext = userContext;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeUserAnswers()
    {
        try
        {
            var userId = _userContext.UserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated." });

            var answersResult = await _questionAnswerServices.GetAnswersForUserAsync(userId);

            if (!answersResult.IsSuccess || answersResult.Answers == null || !answersResult.Answers.Any())
                return BadRequest(new { message = "No answers found for this user. Please submit your answers first." });

            var inputText = FormatAnswersAsText(answersResult.Answers);

            var analysisJson = await _modelService.AnalyzeUserAnswersAsync(inputText);

            var parsedResult = JsonSerializer.Deserialize<AITaskResponse>(analysisJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Failed to parse AI model response.");

            var isAdded = await _taskServices.AddTasksFromModelAsync(parsedResult, userId);

            if (!isAdded)
                _logger.LogWarning("Failed to add tasks for user {UserId}.", userId);

            var markResult = await _patientServices.MarkAsDiagnosedAsync(userId);
            if (!markResult.IsSuccess)
                _logger.LogWarning("Failed to mark patient profile as diagnosed for user {UserId}.", userId);

            return Ok(new
            {
                analysis = parsedResult,
                answersCount = answersResult.Answers.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user answers: {Message}", ex.Message);
            return StatusCode(500, new
            {
                error = "An error occurred while analyzing answers."
            });
        }
    }

    private static string FormatAnswersAsText(IEnumerable<Eirene.DAL.Entities.Treatment.QuestionAnswer> answers)
    {
        var sentences = answers
            .Where(a => !string.IsNullOrWhiteSpace(a.Answer))
            .Select(a =>
            {
                var answer = a.Answer.Trim();
                var combined = $"{answer}";
                return combined.EndsWith('.') || combined.EndsWith('!') || combined.EndsWith('?')
                    ? combined
                    : combined + ".";
            });

        return string.Join(" ", sentences);
    }
}
