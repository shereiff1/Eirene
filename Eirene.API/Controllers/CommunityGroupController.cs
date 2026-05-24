using Eirene.BLL.Enumerators;
using Eirene.BLL.Models.Community.Group;
using Eirene.BLL.Services.Abstraction.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Eirene.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommunityGroupController : ControllerBase
{
    private readonly ILogger<CommunityGroupController> _logger;
    private readonly ICommunityGroupServices _communityGroupServices;
    public CommunityGroupController(ILogger<CommunityGroupController> logger, ICommunityGroupServices communityGroupServices)
    {
        _logger = logger;
        _communityGroupServices = communityGroupServices;
    }
    [HttpGet]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _communityGroupServices.GetAllAsync();
        if (!result.IsSuccess)
            return StatusCode(500, new { message = "Could not retrieve community groups." });
        return Ok(result.Groups);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _communityGroupServices.GetByIdAsync(id);
        if (!result.IsSuccess || result.Group == null)
            return NotFound(new { message = "Community group not found." });
        return Ok(result.Group);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] AddCommunityGroup group)
    {
        var result = await _communityGroupServices.CreateAsync(group);
        if (!result.IsSuccess || result.CreatedGroup == null)
            return BadRequest(new { message = "Failed to create community group." });
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CreatedGroup.Id },
            result.CreatedGroup
        );
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditCommunityGroup group)
    {
        if (group.Id != Guid.Empty && group.Id != id)
        {
            return BadRequest(new { message = "ID mismatch between URL and body." });
        }
        group.Id = id;
        var updated = await _communityGroupServices.UpdateAsync(group);
        if (!updated)
            return NotFound(new { message = "Community group not found." });
        return NoContent();
    }


    [HttpGet("details/{id}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetByIdWithDetails(Guid id)
    {
        var result = await _communityGroupServices.GetByIdWithFullDetailsAsync(id);
        if (!result.IsSuccess || result.Group == null)
            return NotFound(new { message = "Community group not found." });
        return Ok(result.Group);
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _communityGroupServices.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = "Community group not found." });
        return NoContent();
    }

    [HttpPost("{id}/join")]
    [Authorize(Roles = Roles.AllExceptDoctor)]
    public async Task<IActionResult> JoinGroup(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _communityGroupServices.JoinGroupAsync(id, userId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPost("{id}/leave")]
    [Authorize(Roles = Roles.AllExceptDoctor)]
    public async Task<IActionResult> LeaveGroup(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _communityGroupServices.LeaveGroupAsync(id, userId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpGet("my-groups")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetMyGroups()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _communityGroupServices.GetJoinedByUserIdAsync(userId);
        if (!result.IsSuccess)
            return StatusCode(500, new { message = "Could not retrieve joined community groups." });
        return Ok(result.Groups);
    }

    [HttpGet("available-groups")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetAvailableGroups()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _communityGroupServices.GetUnjoinedByUserIdAsync(userId);
        if (!result.IsSuccess)
            return StatusCode(500, new { message = "Could not retrieve available community groups." });
        return Ok(result.Groups);
    }
}
