using Eirene.BLL.Enumerators;
using Eirene.BLL.Models.Community.Post;
using Eirene.BLL.Services.Abstraction.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommunityPostController : ControllerBase
{
    private readonly ILogger<CommunityPostController> _logger;
    private readonly ICommunityPostServices _communityPostServices;
    public CommunityPostController(ILogger<CommunityPostController> logger, ICommunityPostServices communityPostService)
    {
        _logger = logger;
        _communityPostServices = communityPostService;
    }
    [HttpGet]
    [Authorize(Roles = Roles.AllExceptDoctor)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _communityPostServices.GetAllAsync();
        if (!result.IsSuccess)
            return BadRequest("Could not retrieve community posts.");
        return Ok(result.Posts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _communityPostServices.GetByIdAsync(id);
        if (!result.IsSuccess || result.Post == null)
            return NotFound();
        return Ok(result.Post);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetByGroupId(Guid groupId)
    {
        var result = await _communityPostServices.GetByGroupIdAsync(groupId);
        if (!result.IsSuccess)
            return BadRequest("Could not retrieve posts for the group.");
        return Ok(result.Posts);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(string userId)
    {
        var result = await _communityPostServices.GetByUserIdAsync(userId);
        if (!result.IsSuccess)
            return BadRequest("Could not retrieve user's posts.");
        return Ok(result.Posts);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> Create([FromBody] AddCommunityPost post)
    {
        var result = await _communityPostServices.CreateAsync(post);
        if (!result.IsSuccess || result.CreatedPost == null)
            return BadRequest("Failed to create post.");
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CreatedPost.Id },
            result.CreatedPost
        );
    }

    [HttpPut]
    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> Update([FromBody] EditCommunityPost post)
    {
        var updated = await _communityPostServices.UpdateAsync(post);
        if (!updated)
            return NotFound("Post not found.");
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _communityPostServices.DeleteAsync(id);
        if (!deleted)
            return NotFound("Post not found.");
        return NoContent();
    }
}
