using Eirene.BLL.Services.Abstraction.Treatment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eirene.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PatientTaskController : ControllerBase
{
    private readonly IPatientTaskServices _taskServices;
    private readonly ILogger<PatientTaskController> _logger;

    public PatientTaskController(IPatientTaskServices taskServices, ILogger<PatientTaskController> logger)
    {
        _taskServices = taskServices;
        _logger = logger;
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetTasksForUser()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated." });

            var tasks = await _taskServices.GetTasksForUserAsync(userId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for user.");
            return StatusCode(500, new { error = "An error occurred while retrieving tasks." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated." });

            var task = await _taskServices.GetTaskByIdAsync(id);

            if (task == null)
                return NotFound(new { message = "Task not found." });

            if (userId != task.PatientId)
                return Unauthorized(new { message = "This task is not assigned to you" });

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}.", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the task." });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}.", id);
            return StatusCode(500, new { error = "An error occurred while updating the task status." });
        }
    }
}

public class UpdateTaskStatusRequest
{
    public bool IsCompleted { get; set; }
}
