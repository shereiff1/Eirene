using BLL.ModelVMs.Content;
using BLL.Services.Abstraction.Content;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> GetAll()
    {
        var result = await _blogServices.GetAllAsync();
        if (!result.IsSuccess)
            return BadRequest("Could not retrieve blogs.");

        return Ok(result.Posts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _blogServices.GetByIdAsync(id);
        if (!result.IsSuccess || result.Post == null)
            return NotFound();

        return Ok(result.Post);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddBlog blog)
    {
        var result = await _blogServices.CreateAsync(blog);

        if (!result.IsSuccess || result.CreatedPost == null)
            return BadRequest("Failed to create blog.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CreatedPost.Id },
            result.CreatedPost
        );
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] EditBlog blog)
    {
        var updated = await _blogServices.UpdateAsync(blog);

        if (!updated)
            return NotFound("Blog not found.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _blogServices.DeleteAsync(id);

        if (!deleted)
            return NotFound("Blog not found.");

        return NoContent();
    }

}