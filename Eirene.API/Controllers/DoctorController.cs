using System.Security.Claims;
using Eirene.BLL.Enumerators;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.API.Filters;
using Eirene.BLL.Services.Abstraction.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorController : ControllerBase
{
    private readonly ILogger<DoctorController> _logger;
    private readonly IDoctorProfileService _doctorProfileService;
    private readonly ISupervisionService _supervisionService;
    private readonly IDoctorRatingService _doctorRatingService;
    private readonly IPictureService _pictureService;
    private readonly IUserContext _userContext;

    public DoctorController(
        IDoctorProfileService doctorProfileService,
        ISupervisionService supervisionService,
        IDoctorRatingService doctorRatingService,
        IPictureService pictureService,
        IUserContext userContext,
        ILogger<DoctorController> logger)
    {
        _logger = logger;
        _doctorProfileService = doctorProfileService;
        _supervisionService = supervisionService;
        _doctorRatingService = doctorRatingService;
        _pictureService = pictureService;
        _userContext = userContext;
    }

    [HttpGet]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _doctorProfileService.GetAllAsync();
        if (result.IsFailure)
            return BadRequest(new { message = "Could not retrieve Doctors." });
        return Ok(result.Value);
    }

    [HttpPost("profile")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> CreateProfile([FromBody] AddDoctorProfile model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated." });

        var result = await _doctorProfileService.CreateDoctorProfileAsync(model, userId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("profile")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> UpdateProfile([FromBody] EditDoctorProfile model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated." });

        var result = await _doctorProfileService.UpdateDoctorProfileAsync(model, userId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _doctorProfileService.GetByIdAsync(id);
        if (result.IsFailure)
            return NotFound(new { message = "Doctor not found." });

        return Ok(result.Value);
    }

    [HttpGet("supervision-requests")]
    [Authorize(Roles = Roles.Doctor)]
    [VerifiedDoctor]
    public async Task<IActionResult> GetSupervisionRequests()
    {
        var doctorId = _userContext.UserId;
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized(new { message = "User not authenticated." });

        var result = await _supervisionService.GetSupervisionRequestsAsync(doctorId);
        if (result.IsFailure)
            return BadRequest(new { message = "Could not retrieve supervision requests." });

        return Ok(result.Value);
    }

    [HttpPut("supervision-requests/{requestId}")]
    [Authorize(Roles = Roles.Doctor)]
    [VerifiedDoctor]
    public async Task<IActionResult> RespondToSupervisionRequest(string requestId, [FromBody] bool accept)
    {
        var doctorId = _userContext.UserId;
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized(new { message = "User not authenticated." });

        var result = await _supervisionService.RespondToSupervisionRequestAsync(requestId, accept, doctorId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(new
        {
            message = accept ? "Request accepted. Patient is now under your supervision." : "Request declined."
        });
    }

    [HttpPost("upload-picture")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var doctorId = _userContext.UserId;
        if (string.IsNullOrEmpty(doctorId))
            return Unauthorized(new { message = "User not authenticated." });

        var result = await _pictureService.UploadPictureAsync(file);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        var editModel = new EditDoctorProfile
        {
            ProfilePhotoUrl = result.Url
        };
        var finalResult = await _doctorProfileService.UpdateDoctorProfileAsync(editModel, doctorId);
        if (finalResult.IsFailure)
            return BadRequest(finalResult.Error);
        return Ok(new
        {
            message = "Profile picture uploaded successfully.",
            url = result.Url
        });
    }


    [HttpGet("ratings/{doctorId}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetDoctorRatings(string doctorId)
    {
        var result = await _doctorRatingService.GetDoctorRatingsAsync(doctorId);
        if (result.IsFailure)
            return BadRequest(new { message = "Could not retrieve doctor ratings." });

        return Ok(result.Value);
    }

    [HttpDelete("cancel-supervision")]
    [Authorize(Roles = Roles.Doctor)]
    [VerifiedDoctor]
    public async Task<IActionResult> CancelDoctorSupervision()
    {
        var userId = _userContext.UserId;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated." });
        var result = await _supervisionService.RemoveSupervisionOnPatient(userId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });
        return Ok(new { message = "Supervision is cancelled successfully." });
    }

    [HttpGet("profile-picture/{userId}")]
    [Authorize(Roles = Roles.AllUsers)]
    public async Task<IActionResult> GetDoctorProfilePicture(string userId)
    {
        var result = await _doctorProfileService.GetByIdAsync(userId);
        if (!result.IsSuccess)
            return NotFound(new { message = "Doctor not found." });

        var imageUrl = result.Value?.ProfilePhotoUrl;
        if (string.IsNullOrEmpty(imageUrl))
            return NotFound("Profile picture not set.");

        if (!imageUrl.StartsWith("/") &&
            !(Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) &&
              uri.Scheme == "https" && uri.Host.Contains("cloudinary.com")))
            return BadRequest(new { message = "Invalid profile picture URL." });

        return Redirect(imageUrl);
    }

    [HttpGet("patients")]
    [Authorize(Roles = Roles.Doctor)]
    [VerifiedDoctor]
    public async Task<IActionResult> GetDoctorsPatients()
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated." });

        var result = await _supervisionService.GetDoctorsPatientsAsync(userId);
        if (result.IsFailure)
            return BadRequest(new { message = "Could not retrieve patients." });

        return Ok(result.Value);
    }

    [HttpDelete("profile")]
    [Authorize(Roles = Roles.DoctorOrAdmin)]
    public async Task<IActionResult> DeleteDoctorProfile()
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated." });
        var result = await _doctorProfileService.DeleteDoctorProfile(userId);
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });
        return Ok(new { message = "Doctor profile deleted successfully." });
    }
    [HttpGet("is-verified")]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> CheckIfVerified()
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                message = "User not authenticated.",
            });
        }
        var result = await _doctorProfileService.CheckIfVerified(userId);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { isVerified = result.Value });
    }
}