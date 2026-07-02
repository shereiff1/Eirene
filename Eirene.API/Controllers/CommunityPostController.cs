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
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _communityPostServices.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            if (result.Post == null)
                return NotFound(new { message = "Post not found." });
            return StatusCode(403, new { message = "Access denied" });
        }
        return Ok(result.Post);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetByGroupId(Guid groupId)
    {
        var result = await _communityPostServices.GetByGroupIdAsync(groupId);
        if (!result.IsSuccess)
        {
            if (result.Message == "Unauthorized")
                return Unauthorized(new { message = "User not authenticated" });
            if (result.Message == "You are not a member of this community group.")
                return StatusCode(403, new { message = result.Message });
            return StatusCode(500, new { message = result.Message });
        }
        return Ok(result.Posts);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(string userId)
    {
        var result = await _communityPostServices.GetByUserIdAsync(userId);
        if (!result.IsSuccess)
            return StatusCode(500, new { message = "Could not retrieve user's posts." });
        return Ok(result.Posts);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> Create([FromBody] AddCommunityPost post)
    {
        var result = await _communityPostServices.CreateAsync(post);
        if (!result.IsSuccess || result.CreatedPost == null)
            return BadRequest(new { message = result.Message });
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CreatedPost.Id },
            result.CreatedPost
        );
    }

    [HttpPut]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> Update([FromBody] EditCommunityPost post)
    {
        var updated = await _communityPostServices.UpdateAsync(post);
        if (!updated.IsAllowed)
            return BadRequest(new { message = updated.Message });
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _communityPostServices.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = "Post not found." });
        return NoContent();
    }
}
