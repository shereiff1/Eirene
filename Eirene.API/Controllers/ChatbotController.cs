using Eirene.BLL.Models.Communication;
using Eirene.BLL.Services.Abstraction.Communication;
using Eirene.BLL.Services.Abstraction.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;
    private readonly IUserContext _userContext;

    public ChatbotController(IChatbotService chatbotService, IUserContext userContext)
    {
        _chatbotService = chatbotService;
        _userContext = userContext;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatbotSendMessageDto request)
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _chatbotService.SendMessageAsync(userId, request);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("busy") || result.Error.Contains("unavailable"))
                return StatusCode(503, new { message = result.Error });

            if (result.Error.Contains("Access denied"))
                return StatusCode(403, new { message = result.Error });

            if (result.Error.Contains("not found"))
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _chatbotService.GetUserSessionsAsync(userId);

        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("sessions/{sessionId:guid}/messages")]
    public async Task<IActionResult> GetSessionMessages(Guid sessionId)
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _chatbotService.GetSessionMessagesAsync(userId, sessionId);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("Access denied"))
                return StatusCode(403, new { message = result.Error });

            if (result.Error.Contains("not found"))
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> DeleteSession(Guid sessionId)
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _chatbotService.DeleteSessionAsync(userId, sessionId);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("Access denied"))
                return StatusCode(403, new { message = result.Error });

            if (result.Error.Contains("not found"))
                return NotFound(new { message = result.Error });

            return BadRequest(new { message = result.Error });
        }

        return NoContent();
    }
}
