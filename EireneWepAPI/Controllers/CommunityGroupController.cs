using BLL.Enumerators;
using BLL.Models.Community.Group;
using BLL.Services.Abstraction.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Eirene.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommunityGroupController : ControllerBase
    {
        private readonly ILogger<CommunityGroupController> _logger;
        private readonly ICommunityGroupServices _communityGroupServices;
        public CommunityGroupController(ILogger<CommunityGroupController> logger, ICommunityGroupServices communityGroupServices)
        {
            _logger = logger;
            _communityGroupServices = communityGroupServices;
        }
        [HttpGet]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _communityGroupServices.GetAllAsync();
            if (!result.IsSuccess)
                return BadRequest("Could not retrieve community groups.");
            return Ok(result.Groups);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _communityGroupServices.GetByIdAsync(id);
            if (!result.IsSuccess || result.Group == null)
                return NotFound();
            return Ok(result.Group);
        }
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Create([FromBody] AddCommunityGroup group)
        {
            var result = await _communityGroupServices.CreateAsync(group);
            if (!result.IsSuccess || result.CreatedGroup == null)
                return BadRequest("Failed to create community group.");
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.CreatedGroup.Id },
                result.CreatedGroup
            );
        }

        [HttpPut]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Update([FromBody] EditCommunityGroup group)
        {
            var updated = await _communityGroupServices.UpdateAsync(group);
            if (!updated)
                return NotFound("Community group not found.");
            return NoContent();
        }


        [HttpGet("details/{id}")]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> GetByIdWithDetails(int id)
        {
            var result = await _communityGroupServices.GetByIdWithFullDetailsAsync(id);
            if (!result.IsSuccess || result.Group == null)
                return NotFound();
            return Ok(result.Group);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _communityGroupServices.DeleteAsync(id);
            if (!deleted)
                return NotFound("Community group not found.");
            return NoContent();
        }
    }
}
