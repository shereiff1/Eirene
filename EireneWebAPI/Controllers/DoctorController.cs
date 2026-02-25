using System.Security.Claims;
using BLL.Enumerators;
using BLL.Models.Core.Doctor;
using BLL.ModelVMs.Content;
using BLL.Services.Abstraction.Content;
using BLL.Services.Abstraction.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorController : ControllerBase
{
    private readonly ILogger<DoctorController> _logger;
    private readonly IDoctorServices _services;
    private readonly IPictureService _pictureService;

    public DoctorController(IDoctorServices services, IPictureService pictureService, ILogger<DoctorController> logger)
    {
        _logger = logger;
        _services = services;
        _pictureService = pictureService;
    }

    [HttpGet]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetAll()
    {
        var results = await _services.GetAllAsync();
        if (!results.IsSuccess)
            return BadRequest("Could not retrieve Doctors.");
        return Ok(results.Doctors); 
    }

    [HttpPost("profile")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> CreateProfile([FromBody] AddDoctorProfile model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated.");

        var result = await _services.CreateDoctorProfileAsync(model, userId);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Doctor);
    }

    [HttpPut("profile")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> UpdateProfile([FromBody] EditDoctorProfile model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated.");

        var result = await _services.UpdateDoctorProfileAsync(model, userId);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Doctor);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _services.GetByIdAsync(id);
        if (!result.isSuccess)
            return NotFound("Doctor not found.");
        
        return Ok(result.Doctor);
    }

    [HttpGet("supervision-requests")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> GetSupervisionRequests()
    {
        var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized("User not authenticated.");

        var result = await _services.GetSupervisionRequestsAsync(doctorId);
        if (!result.IsSuccess)
            return BadRequest("Could not retrieve supervision requests.");

        return Ok(result.Requests);
    }

    [HttpPut("supervision-requests/{requestId}")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> RespondToSupervisionRequest(string requestId, [FromBody] bool accept)
    {
        var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized("User not authenticated.");

        var result = await _services.RespondToSupervisionRequestAsync(requestId, accept, doctorId);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new { message = accept ? "Request accepted. Patient is now under your supervision." : "Request declined." });
    }

    [HttpPost("upload-picture")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized("User not authenticated.");

        var result = await _pictureService.UploadPictureAsync(file);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        var editModel = new EditDoctorProfile
        {
            ProfilePhotoUrl = result.Url
        };
        var finalResult = await _services.UpdateDoctorProfileAsync(editModel, doctorId);
        if (!finalResult.IsSuccess)
            return BadRequest(finalResult.Error);
        return Ok(new { 
            message = "Profile picture uploaded successfully.",
            url = result.Url });
    }

    
    [HttpDelete("cancel-supervision")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> CancelDoctorSupervision()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated.");
        var result =  await _services.RemoveSupervisionOnPatient(userId);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok(new { message = "Supervision is cancelled successfully." });
    }
        
}