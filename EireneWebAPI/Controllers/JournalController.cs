using BLL.Enumerators;
using BLL.Models.Tracking;
using BLL.Services.Abstraction.Tracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JournalController : ControllerBase
    {
        private readonly IJournalServices _journalService;
        private readonly ILogger<JournalController> _logger;

        public JournalController(IJournalServices journalService, ILogger<JournalController> logger)
        {
            _journalService = journalService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> AddJournal([FromBody] AddJournal journal)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _journalService.CreateAsync(journal);

            if (!result.IsSuccess || result.AddedJournal == null)
                return BadRequest(new
                    { message = "Failed to create the journal. You may have already created a journal today." });

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.AddedJournal.Id },
                result.AddedJournal
            );
        }

        [HttpGet]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _journalService.GetAllAsync();

            if (!result.IsSuccess || result.journals == null)
                return StatusCode(500, new { message = "Failed to retrieve journals" });

            return Ok(result.journals);
        }

        [HttpGet("Can-create-today")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> CanCreateToday()
        {
            var canCreate = await _journalService.CanCreateToday();

            if (!canCreate)
                return Ok(false);
            return Ok(true);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _journalService.GetByIdAsync(id);

            if (!result.IsSuccess || result.journal == null)
                return NotFound(new { message = "Journal not found" });

            return Ok(result.journal);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> DeleteJournal(int id)
        {
            var isSuccess = await _journalService.DeleteAsync(id);
            if (!isSuccess)
                return BadRequest(new { message = "Failed to delete journal." });
            return NoContent();
        }


        [HttpPut]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> UpdateJournal([FromBody] EditJournal journal)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isSuccess = await _journalService.UpdateAsync(journal);

            if (!isSuccess)
                return BadRequest(new { message = "Failed to update journal. Only today's journal can be edited." });

            return NoContent();
        }
    }
}