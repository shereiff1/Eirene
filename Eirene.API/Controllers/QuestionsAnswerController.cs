using Eirene.BLL.Models.Treatment.Question;
using Eirene.BLL.Services.Abstraction.Treatment;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eirene.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsAnswerController : ControllerBase
    {
        private readonly ILogger<QuestionsAnswerController> _logger;
        private readonly IQuestionAnswerServices _questionAnswerServices;
        private readonly IQuestionServices _questionServices;


        public QuestionsAnswerController(
            ILogger<QuestionsAnswerController> logger,
            IQuestionAnswerServices questionAnswerServices,
            IQuestionServices questionServices)
        {
            _logger = logger;
            _questionAnswerServices = questionAnswerServices;
            _questionServices = questionServices;
        }

        [HttpGet("questions")]
        public async Task<IActionResult> GetAllQuestions()
        {
            try
            {
                var (IsSuccess, Questions) = await _questionServices.GetAllAsync();
                if (!IsSuccess || Questions == null || !Questions.Any())
                {
                    return NotFound(new { message = "No questions found." });
                }

                return Ok(Questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions");
                return StatusCode(500, new { message = "An error occurred while retrieving questions." });
            }
        }

        [HttpGet("my-answers")]
        public async Task<IActionResult> GetMyAnswers()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                var (IsSuccess, Answers) = await _questionAnswerServices.GetAnswersForUserAsync(userId);

                if (Answers == null || !Answers.Any())
                {
                    return Ok(new { message = "No answers found for this user." });
                }

                if (!IsSuccess)
                {
                    return StatusCode(500, new { message = "An error occurred while retrieving answers." });
                }

                return Ok(Answers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving answers");
                return StatusCode(500, new { message = "An error occurred while retrieving answers." });
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAnswers([FromBody] QuestionsAnswer questionsAnswer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                if (questionsAnswer.Answers == null || !questionsAnswer.Answers.Any())
                {
                    return BadRequest(new { message = "No answers provided." });
                }

                var answersToAdd = questionsAnswer.Answers
                    .Select(a => (a.QuestionId, a.Answer))
                    .ToList();

                var (IsSuccess, Answers) = await _questionAnswerServices.AddMultipleAnswersAsync(userId, answersToAdd);

                if (!IsSuccess)
                {
                    return BadRequest(new { message = "Failed to save some or all answers." });
                }

                return Ok(new { Message = "Answers submitted successfully", Answers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting answers");
                return StatusCode(500, new { message = "An error occurred while submitting answers." });
            }
        }
    }
}