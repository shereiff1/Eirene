using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Eirene.BLL.Models.Community.Membership;
using Eirene.BLL.Models.Core.Admin;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.BLL.Services.Abstraction.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EireneWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminProfileService _adminProfileService;
    private readonly IRoleManagementService _roleManagementService;
    private readonly ICommunityModerationService _communityModerationService;
    private readonly IUserContext _userContext;

    public AdminController(
        IAdminProfileService adminProfileService,
        IRoleManagementService roleManagementService,
        ICommunityModerationService communityModerationService,
        IUserContext userContext)
    {
        _adminProfileService = adminProfileService;
        _roleManagementService = roleManagementService;
        _communityModerationService = communityModerationService;
        _userContext = userContext;
    }



    [HttpPost("profile")]
    public async Task<IActionResult> CreateAdminProfile()
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated." });

        var result = await _adminProfileService.CreateAdminProfileAsync(userId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _adminProfileService.GetByIdAsync(id);
        if (result.IsFailure)
            return NotFound(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("profiles")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _adminProfileService.GetAllAsync();
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }


    [HttpPost("roles/assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
    {
        if (string.IsNullOrWhiteSpace(model.userId) || string.IsNullOrWhiteSpace(model.role))
            return BadRequest(new { message = "User ID and Role are required." });

        var result = await _roleManagementService.AssignRoleAsync(_userContext.UserId ?? string.Empty, model.userId, model.role);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(new { Message = $"Role '{model.role}' successfully assigned to user '{model.userId}'." });
    }

    [HttpDelete("community-groups/{groupId}/members/{userId}")]
    public async Task<IActionResult> RemoveUserFromGroup(Guid groupId, string userId)
    {
        var result = await _communityModerationService.ManageCommunityGroupMembershipAsync(groupId, userId, assign: false);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(new { Message = $"User '{userId}' successfully removed from group '{groupId}'." });
    }

    [HttpPost("community-group/{groupId}/ban")]
    public async Task<IActionResult> BanUserFromGroup(Guid groupId, [FromBody] CommunityGroupUserActionRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.UserId))
            return BadRequest(new { message = "User ID is required." });

        var result = await _communityModerationService.BanUserFromGroupAsync(groupId, model.UserId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(new { Message = "User was banned from the community group successfully." });
    }

    [HttpPost("community-group/{groupId}/unban")]
    public async Task<IActionResult> UnbanUserFromGroup(Guid groupId, [FromBody] CommunityGroupUserActionRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.UserId))
            return BadRequest(new { message = "User ID is required." });

        var result = await _communityModerationService.UnbanUserFromGroupAsync(groupId, model.UserId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(new { Message = "User was unbanned from the community group successfully." });
    }

    [HttpPost("community-group/{groupId}/timeout")]
    public async Task<IActionResult> TimeoutUserInGroup(Guid groupId, [FromBody] CommunityGroupUserTimeoutRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.UserId))
            return BadRequest(new { message = "User ID is required." });

        var result = await _communityModerationService.TimeoutUserInGroupAsync(groupId, model.UserId, model.TimeoutUntil);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(new { Message = "User timeout was applied successfully." });
    }

    [HttpPost("community-group/{groupId}/timeout/remove")]
    public async Task<IActionResult> RemoveTimeoutUserInGroup(Guid groupId, [FromBody] CommunityGroupUserActionRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.UserId))
            return BadRequest(new { message = "User ID is required." });

        var result = await _communityModerationService.RemoveTimeoutUserInGroupAsync(groupId, model.UserId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(new { Message = "User timeout was removed successfully." });
    }

    [HttpGet("community-group/{groupId}/banned-users")]
    public async Task<IActionResult> GetBannedUsers(Guid groupId)
    {
        var result = await _communityModerationService.GetBannedUsersByGroupAsync(groupId);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("community-group/{groupId}/timed-out-users")]
    public async Task<IActionResult> GetTimedOutUsers(Guid groupId)
    {
        var result = await _communityModerationService.GetTimedOutUsersByGroupAsync(groupId);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("doctors/pending")]
    public async Task<IActionResult> GetPendingDoctors()
    {
        var result = await _roleManagementService.GetPendingDoctorsAsync();
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("doctors/{doctorId}/approve")]
    public async Task<IActionResult> ApproveDoctor(string doctorId)
    {
        var result = await _roleManagementService.ApproveDoctorAsync(doctorId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(new { Message = "Doctor has been successfully verified." });
    }
}
