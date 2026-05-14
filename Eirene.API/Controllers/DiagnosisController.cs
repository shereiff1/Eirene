using Eirene.BLL.AIModel;
using Eirene.BLL.Services.Abstraction.Treatment;
using Eirene.DAL.Entities.Treatment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace Eirene.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DiagnosisController : ControllerBase
    {
        private readonly IAIModelService _modelService;
        private readonly ILogger<DiagnosisController> _logger;
        private readonly IQuestionAnswerServices _questionAnswerServices;
        private readonly IQuestionServices _questionServices;
        private readonly IPatientTaskServices _taskServices;

        public DiagnosisController(
            IAIModelService ModelService,
            ILogger<DiagnosisController> logger,
            IQuestionAnswerServices questionAnswerServices,
            IQuestionServices questionServices,
            IPatientTaskServices taskServices)
        {
            _modelService = ModelService;
            _logger = logger;
            _questionAnswerServices = questionAnswerServices;
            _questionServices = questionServices;
            _taskServices = taskServices;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeUserAnswers()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                var answersResult = await _questionAnswerServices.GetAnswersForUserAsync(userId);

                if (!answersResult.IsSuccess || answersResult.Answers == null || !answersResult.Answers.Any())
                {
                    return BadRequest(new { message = "No answers found for this user. Please submit your answers first." });
                }

                var formattedQA = await FormatQuestionsAndAnswers(answersResult.Answers);

                var analysisResult = await _modelService.AnalyzeUserAnswersAsync(formattedQA);
                var IsAdded = await _taskServices.AddTasksFromModelAsync(analysisResult, userId);

                if (!IsAdded)
                {
                    _logger.LogWarning("Failed to add tasks for user {UserId} based on analysis.", userId);
                }

                return Ok(new
                {
                    analysis = analysisResult,
                    answersCount = answersResult.Answers.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing user answers. Details: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "An error occurred while analyzing answers.",
                    details = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
        private async Task<string> FormatQuestionsAndAnswers(IEnumerable<QuestionAnswer> answers)
        {
            var sb = new StringBuilder();
            int index = 1;

            foreach (var answer in answers)
            {
                var questionResult = await _questionServices.GetByIdAsync(answer.QuestionId);

                if (questionResult.IsSuccess && questionResult.question != null)
                {
                    sb.AppendLine($"Q{index}: {questionResult.question.QuestionContent}");
                    sb.AppendLine($"A{index}: {answer.Answer}");
                    sb.AppendLine();
                    index++;
                }
            }

            return sb.ToString();
        }
    }
}