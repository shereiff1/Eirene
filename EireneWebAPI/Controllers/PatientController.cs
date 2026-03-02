using BLL.Enumerators;
using BLL.Services.Abstraction.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BLL.Models.Core.Doctor;
using BLL.Models.Core.Patient;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Eirene.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IPatientServices _patientServices;
        private readonly IPictureService _pictureService;
        private readonly ILogger<PatientController> _logger;

        public PatientController(IPatientServices patientServices, 
            IPictureService pictureService, 
            ILogger<PatientController> logger, 
            IWebHostEnvironment webHostEnvironment, 
            IConfiguration configuration)
        {
            _patientServices = patientServices;
            _pictureService = pictureService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> GetAll()
        {
            var results = await _patientServices.GetAllAsync();
            if (!results.IsSuccess)
                return BadRequest("Could not retrieve Patients.");
            return Ok(results.Patients); 
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _patientServices.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound("Patient not found.");
            
            return Ok(result.Patient);
        }

        [HttpPost("profile")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> CreatePatientProfile([FromBody] AddPatientProfile model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _patientServices.CreatePatientProfileAsync(model, userId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Patient);
        }

        [HttpPut("profile")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> UpdatePatientProfile([FromBody] EditPatientProfile model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _patientServices.UpdatePatientProfileAsync(model, userId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Patient);
        }

        [HttpDelete("profile")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> DeletePatientProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _patientServices.DeletePatientProfileAsync(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new { message = "Patient profile deleted successfully." });
        }

        [HttpPut("request-doctor/{doctorId}")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> RequestSupervision(string doctorId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _patientServices.RequestSupervisionAsync(userId, doctorId);

            if (!result.IsSuccess)  
                return BadRequest(result.Error);

            return Ok(new { message = "Supervision request sent. Waiting for doctor approval." });
        }
        
        [HttpPost("upload-picture")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> UploadProfilePicture(IFormFile pictureFile)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _pictureService.UploadPictureAsync(pictureFile);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            var editModel = new EditPatientProfile
            {
                ProfilePhotoUrl = result.Url
            };
            var finalResult = await _patientServices.UpdatePatientProfileAsync(editModel, userId);
            if (!finalResult.IsSuccess)
                return BadRequest(finalResult.Error);
            return Ok(new { 
                message = "Profile picture uploaded successfully.",
                url = result.Url });
        }

        [HttpDelete("cancel-supervision")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> CancelDoctorSupervision()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");
            var result =  await _patientServices.RemoveDoctorSupervision(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok(new { message = "Supervision is cancelled successfully." });
        }

        [HttpPost("rate-supervisor/{doctorId}")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> RateSupervisor(string doctorId, [FromBody] AddDoctorRatingDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _patientServices.RateSupervisorAsync(userId, doctorId, model);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new { message = "Thanks for rating your assigned doctor." });
        }
        
        [HttpGet("profile-picture/{userId}")]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> getDoctorProfilePicture(string userId)
        {
            var result = await _patientServices.GetByIdAsync(userId);
            if (!result.IsSuccess)
                return NotFound("Patient not found.");

            var fileName = result.Patient?.ProfilePhotoUrl;
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
    }
}
