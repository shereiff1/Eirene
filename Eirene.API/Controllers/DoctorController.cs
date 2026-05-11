using System.Security.Claims;
using Eirene.BLL.Enumerators;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DoctorController> _logger;
        private readonly IDoctorServices _services;
        private readonly IPictureService _pictureService;

        public DoctorController(IDoctorServices services, IPictureService pictureService,
            ILogger<DoctorController> logger, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _pictureService = pictureService;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
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
        [VerifiedDoctor]
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
        [VerifiedDoctor]
        public async Task<IActionResult> RespondToSupervisionRequest(string requestId, [FromBody] bool accept)
        {
            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                return Unauthorized("User not authenticated.");

            var result = await _services.RespondToSupervisionRequestAsync(requestId, accept, doctorId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new
            {
                message = accept ? "Request accepted. Patient is now under your supervision." : "Request declined."
            });
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
            return Ok(new
            {
                message = "Profile picture uploaded successfully.",
                url = result.Url
            });
        }


        [HttpGet("ratings/{doctorId}")]
        [Authorize(Roles = Roles.AllUsers)]
        [VerifiedDoctor]
        public async Task<IActionResult> GetDoctorRatings(string doctorId)
        {
            var result = await _services.GetDoctorRatingsAsync(doctorId);
            if (!result.IsSuccess)
                return BadRequest("Could not retrieve doctor ratings.");

            return Ok(result.Ratings);
        }

        [HttpDelete("cancel-supervision")]
        [Authorize(Roles = Roles.Doctor)]
        [VerifiedDoctor]
        public async Task<IActionResult> CancelDoctorSupervision()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");
            var result = await _services.RemoveSupervisionOnPatient(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok(new { message = "Supervision is cancelled successfully." });
        }

        [HttpGet("profile-picture/{userId}")]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> GetDoctorProfilePicture(string userId)
        {
            var result = await _services.GetByIdAsync(userId);
            if (!result.isSuccess)
                return NotFound("Doctor not found.");

            var fileName = result.Doctor?.ProfilePhotoUrl;
            if (string.IsNullOrEmpty(fileName))
                return NotFound("Profile picture not set.");

            var relativePath = fileName.StartsWith("/") ? fileName.Substring(1) : fileName;

            var path = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                relativePath.Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (!System.IO.File.Exists(path))
                return NotFound("Profile picture file not found on server.");

            var imageBytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(imageBytes, "image/jpeg");
        }

        [HttpGet("patients")]
        [Authorize(Roles = Roles.Doctor)]
        [VerifiedDoctor]
        public async Task<IActionResult> GetDoctorsPatients()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _services.GetDoctorsPatientsAsync(userId);
            if (!result.IsSuccess)
                return BadRequest("Could not retrieve patients.");

            return Ok(result.Patients);
        }

        [HttpDelete("profile")]
        [Authorize(Roles = Roles.DoctorOrAdmin)]
        public async Task<IActionResult> DeleteDoctorProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");
            var result = await _services.DeleteDoctorProfile(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok(new { message = "Doctor profile deleted successfully." });
        }
        [HttpGet("is-verified")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> CheckIfVerified()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "User not authenticated.",
                });
            }
            var result = await _services.CheckIfVerified(userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(new { isVerified = result.IsVerified });
        }
    }
}