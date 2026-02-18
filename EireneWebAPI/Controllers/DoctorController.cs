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

    public DoctorController(IDoctorServices services, ILogger<DoctorController> logger)
    {
        _logger = logger;
        _services = services;
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
    [Authorize(Roles = Roles.AllUsers)]
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
}