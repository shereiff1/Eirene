using BLL.Models.Treatment.Question;
using BLL.Services.Abstraction.Treatment; 
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
                    return NotFound("No questions found.");
                }
                return Ok(Questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions");
                return StatusCode(500, "An error occurred while retrieving questions.");
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
                    return Unauthorized("User not authenticated.");
                }

                var (IsSuccess, Answers) = await _questionAnswerServices.GetAnswersForUserAsync(userId);

                if (!IsSuccess || Answers == null || !Answers.Any())
                {
                    return NotFound("No answers found for this user.");
                }

                return Ok(Answers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving answers");
                return StatusCode(500, "An error occurred while retrieving answers.");
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
                    return Unauthorized("User not authenticated.");
                }

                if (questionsAnswer.Answers == null || !questionsAnswer.Answers.Any())
                {
                    return BadRequest("No answers provided.");
                }

                var answersToAdd = questionsAnswer.Answers
                    .Select(a => (a.QuestionId, a.Answer))
                    .ToList();

                var (IsSuccess, Answers) = await _questionAnswerServices.AddMultipleAnswersAsync(userId, answersToAdd);

                if (!IsSuccess)
                {
                    return BadRequest("Failed to save some or all answers.");
                }

                return Ok(new { Message = "Answers submitted successfully", Answers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting answers");
                return StatusCode(500, "An error occurred while submitting answers.");
            }
        }
    }
}