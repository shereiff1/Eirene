using BLL.Enumerators;
using BLL.Services.Abstraction.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BLL.Models.Core.Patient;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Eirene.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IPatientServices _patientServices;
        private readonly ILogger<PatientController> _logger;

        public PatientController(IPatientServices patientServices, ILogger<PatientController> logger)
        {
            _patientServices = patientServices;
            _logger = logger;
        }

        [HttpPut("request-doctor/{doctorId}")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> AssignDoctor(string doctorId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _patientServices.AssignDoctorAsync(userId, doctorId);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new { message = "Doctor assigned successfully." });
        }
        // [HttpPost("profile")]
        // [Authorize(Roles = Roles.Patient)]
        // public async Task<IActionResult> CreatePatientProfile([FromBody] AddPatientProfile model)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);
        //
        //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     if (string.IsNullOrEmpty(userId))
        //         return Unauthorized("User not authenticated.");
        //
        //     var result = await _patientServices.CreatePatientProfileAsync(model, userId);
        //     if (!result.IsSuccess)
        //         return BadRequest(result.Error);
        //
        //     return Ok(result.Patient);
        // }
    }
}
