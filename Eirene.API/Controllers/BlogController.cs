using Eirene.BLL.Enumerators;
using Eirene.BLL.ModelVMs.Content;
using Eirene.BLL.Services.Abstraction.Content;
using Eirene.BLL.Services.Abstraction.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BlogController : ControllerBase
{
    private readonly ILogger<BlogController> _logger;
    private readonly IBlogServices _blogServices;
    private readonly IUserContext _userContext;

    public BlogController(IBlogServices blogServices, ILogger<BlogController> logger, IUserContext userContext)
    {
        _blogServices = blogServices;
        _logger = logger;
        _userContext = userContext;
    }

    [HttpGet]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _blogServices.GetAllAsync(page, pageSize);
        if (!result.IsSuccess)
            return BadRequest(new { message = "Could not retrieve blogs." });

        return Ok(result.Posts);
    }

    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetByDoctorId(string doctorId)
    {
        var result = await _blogServices.GetByDoctorIdAsync(doctorId);
        if (!result.IsSuccess)
            return BadRequest(new { message = "Could not retrieve blogs." });

        return Ok(result.Posts);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _blogServices.GetByIdAsync(id);
        if (!result.IsSuccess || result.Post == null)
            return NotFound(new { message = "Blog not found." });

        return Ok(result.Post);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> Create([FromBody] AddBlog blog)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var doctorId = _userContext.UserId;
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized(new { message = "User not authenticated." });

        var result = await _blogServices.CreateAsync(blog, doctorId);

        if (!result.IsSuccess || result.CreatedPost == null)
            return BadRequest(new { message = "Failed to create blog." });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CreatedPost.Id },
            result.CreatedPost
        );
    }

    [HttpPut]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> Update([FromBody] EditBlog blog)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var doctorId = _userContext.UserId;
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized(new { message = "User not authenticated." });

        blog.DoctorId = doctorId;

        var updated = await _blogServices.UpdateAsync(blog);

        if (!updated)
            return NotFound(new { message = "Blog not found." });

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.DoctorOrAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _blogServices.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Blog not found." });

        return NoContent();
    }

}
