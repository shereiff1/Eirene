using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Abstraction.Treatment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PatientTaskController : ControllerBase
{
    private readonly IPatientTaskServices _taskServices;
    private readonly IUserContext _userContext;

    public PatientTaskController(IPatientTaskServices taskServices, IUserContext userContext)
    {
        _taskServices = taskServices;
        _userContext = userContext;
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetTasksForUser()
    {
        var userId = _userContext.UserId;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated." });

        var tasks = await _taskServices.GetTasksForUserAsync(userId);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated." });

        var task = await _taskServices.GetTaskByIdAsync(id);

        if (task == null)
            return NotFound(new { message = "Task not found." });

        if (userId != task.PatientId)
            return StatusCode(403, new { message = "This task is not assigned to you." });

        return Ok(task);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated." });

        var task = await _taskServices.GetTaskByIdAsync(id);
        if (task == null)
            return NotFound(new { message = "Task not found." });

        if (userId != task.PatientId)
            return StatusCode(403, new { message = "You can only update your own tasks." });

        var success = await _taskServices.UpdateTaskStatusAsync(id, request.IsCompleted);

        if (!success)
            return BadRequest(new { message = "Task could not be updated." });

        return Ok(new { message = "Task status updated successfully." });
    }
}

public class UpdateTaskStatusRequest
{
    public bool IsCompleted { get; set; }
}
