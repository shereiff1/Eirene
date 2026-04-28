using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Eirene.BLL.Models.Community.Membership;
using Eirene.BLL.Models.Core.Admin;
using Eirene.BLL.Services.Abstraction.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EireneWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminServices _adminServices;

        public AdminController(IAdminServices adminServices)
        {
            _adminServices = adminServices;
        }

        private string GetCurrentUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;


        [HttpPost("profile")]
        public async Task<IActionResult> CreateAdminProfile()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _adminServices.CreateAdminProfileAsync(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Admin);
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _adminServices.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound($"Admin profile with ID '{id}' not found.");

            return Ok(result.Admin);
        }

        [HttpGet("profiles")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _adminServices.GetAllAsync();
            if (!result.IsSuccess)
                return BadRequest("Could not retrieve admin profiles.");

            return Ok(result.Admins);
        }


        [HttpPost("roles/assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
        {
            if (string.IsNullOrWhiteSpace(model.userId) || string.IsNullOrWhiteSpace(model.role))
                return BadRequest("User ID and Role are required.");

            var success = await _adminServices.AssignRoleAsync(GetCurrentUserId(), model.userId, model.role);
            if (!success)
                return BadRequest("Failed to assign role. (Note: You cannot modify your own role).");

            return Ok(new { Message = $"Role '{model.role}' successfully assigned to user '{model.userId}'." });
        }
        
        [HttpDelete("community-groups/{groupId}/members/{userId}")]
        public async Task<IActionResult> RemoveUserFromGroup(Guid groupId, string userId)
        {
            var success = await _adminServices.ManageCommunityGroupMembershipAsync(groupId, userId, assign: false);
            if (!success)
                return BadRequest("Failed to remove user from the community group.");

            return Ok(new { Message = $"User '{userId}' successfully removed from group '{groupId}'." });
        }

        [HttpPost("community-group/{groupId}/ban")]
        public async Task<IActionResult> BanUserFromGroup(Guid groupId, [FromBody] CommunityGroupUserActionRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId))
                return BadRequest("User ID is required.");

            var result = await _adminServices.BanUserFromGroupAsync(groupId, model.UserId);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { result.Message });
        }

        [HttpPost("community-group/{groupId}/unban")]
        public async Task<IActionResult> UnbanUserFromGroup(Guid groupId, [FromBody] CommunityGroupUserActionRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId))
                return BadRequest("User ID is required.");

            var result = await _adminServices.UnbanUserFromGroupAsync(groupId, model.UserId);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { result.Message });
        }

        [HttpPost("community-group/{groupId}/timeout")]
        public async Task<IActionResult> TimeoutUserInGroup(Guid groupId, [FromBody] CommunityGroupUserTimeoutRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId))
                return BadRequest("User ID is required.");

            var result = await _adminServices.TimeoutUserInGroupAsync(groupId, model.UserId, model.TimeoutUntil);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { result.Message });
        }

        [HttpPost("community-group/{groupId}/timeout/remove")]
        public async Task<IActionResult> RemoveTimeoutUserInGroup(Guid groupId, [FromBody] CommunityGroupUserActionRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId))
                return BadRequest("User ID is required.");

            var result = await _adminServices.RemoveTimeoutUserInGroupAsync(groupId, model.UserId);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { result.Message });
        }

        [HttpGet("community-group/{groupId}/banned-users")]
        public async Task<IActionResult> GetBannedUsers(Guid groupId)
        {
            var users = await _adminServices.GetBannedUsersByGroupAsync(groupId);
            return Ok(users);
        }

        [HttpGet("community-group/{groupId}/timed-out-users")]
        public async Task<IActionResult> GetTimedOutUsers(Guid groupId)
        {
            var users = await _adminServices.GetTimedOutUsersByGroupAsync(groupId);
            return Ok(users);
        }
    }
}
