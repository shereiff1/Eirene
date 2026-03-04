using Eirene.BLL.Enumerators;
using Eirene.BLL.ModelVMs.Content;
using Eirene.BLL.Services.Abstraction.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eirene.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BlogController : ControllerBase
{
    private readonly ILogger<BlogController> _logger;
    private readonly IBlogServices _blogServices;

    public BlogController(IBlogServices blogServices, ILogger<BlogController> logger)
    {
        _blogServices = blogServices;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _blogServices.GetAllAsync();
        if (!result.IsSuccess)
            return BadRequest("Could not retrieve blogs.");

        return Ok(result.Posts);
    }

    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetByDoctorId(string doctorId)
    {
        var result = await _blogServices.GetByDoctorIdAsync(doctorId);
        if (!result.IsSuccess)
            return BadRequest("Could not retrieve blogs.");

        return Ok(result.Posts);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _blogServices.GetByIdAsync(id);
        if (!result.IsSuccess || result.Post == null)
            return NotFound();

        return Ok(result.Post);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> Create([FromBody] AddBlog blog)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized("User ID not found.");

        var result = await _blogServices.CreateAsync(blog, doctorId);

        if (!result.IsSuccess || result.CreatedPost == null)
            return BadRequest("Failed to create blog.");

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

        var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized("User ID not found.");

        blog.DoctorId = doctorId;

        var updated = await _blogServices.UpdateAsync(blog);

        if (!updated)
            return NotFound("Blog not found.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _blogServices.DeleteAsync(id);

        if (!deleted)
            return NotFound("Blog not found.");

        return NoContent();
    }

}
