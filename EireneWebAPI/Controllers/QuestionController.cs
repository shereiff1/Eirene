using BLL.Enumerators;
using BLL.ModelVMs.Treatment;
using BLL.Services.Abstraction.Treatment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly ILogger<QuestionController> _logger;
        private readonly IQuestionServices _questionServices;

        public QuestionController(ILogger<QuestionController> logger, IQuestionServices questionServices)
        {
            _logger = logger;
            _questionServices = questionServices;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _questionServices.GetAllAsync();
            if (result.questions == null || !result.IsSuccess)
            {
                return BadRequest("Could not retrieve blogs.");
            }
            return Ok(result.questions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _questionServices.GetByIdAsync(id);
            if (!result.IsSuccess || result.question == null)
                return NotFound();
            return Ok(result.question);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddQuestion question)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _questionServices.CreateAsync(question);
            if (!result.IsSuccess || result.AddedQuestion == null)
                return BadRequest("Failed to create question.");
            return CreatedAtAction(
                nameof(Create),
                new { id = result.AddedQuestion.Id },
                result.AddedQuestion
            );
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EditQuestion question)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _questionServices.UpdateAsync(question);
            if (!result.IsSuccess || result.editedQuestion == null)
                return BadRequest("Failed to update question.");
            return Ok(result.editedQuestion);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _questionServices.DeleteAsync(id);
            if (!result)
                return BadRequest("Failed to delete question.");
            return Ok(result);
        }
    }
}
