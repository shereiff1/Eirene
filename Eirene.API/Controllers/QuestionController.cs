using Eirene.BLL.Enumerators;
using Eirene.BLL.ModelVMs.Treatment;
using Eirene.BLL.Services.Abstraction.Treatment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
            return BadRequest(new { message = "Could not retrieve Questions." });
        }
        return Ok(result.questions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _questionServices.GetByIdAsync(id);
        if (!result.IsSuccess || result.question == null)
            return NotFound(new { message = "Question not found." });
        return Ok(result.question);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] AddQuestion question)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _questionServices.CreateAsync(question);
        if (!result.IsSuccess || result.AddedQuestion == null)
            return BadRequest(new { message = "Failed to create question." });
        return CreatedAtAction(
            nameof(Create),
            new { id = result.AddedQuestion.Id },
            result.AddedQuestion
        );
    }

    [HttpPut]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update([FromBody] EditQuestion question)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _questionServices.UpdateAsync(question);
        if (!result.IsSuccess || result.editedQuestion == null)
            return BadRequest(new { message = "Failed to update question." });
        return Ok(result.editedQuestion);
    }
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _questionServices.DeleteAsync(id);
        if (!result)
            return BadRequest(new { message = "Failed to delete question." });
        return Ok(result);
    }
}
