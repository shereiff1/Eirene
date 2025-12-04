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
        public async Task<IActionResult> AddJournal([FromBody] AddJournal journal)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _journalService.CreateAsync(journal);

            if (!result.IsSuccess || result.AddedJournal == null)
                return BadRequest(new { message = "Failed to create the journal. You may have already created a journal today." });

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.AddedJournal.Id },
                result.AddedJournal
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _journalService.GetAllAsync();

            if (!result.IsSuccess || result.journals == null)
                return StatusCode(500, new { message = "Failed to retrieve journals" });

            return Ok(result.journals);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _journalService.GetByIdAsync(id);

            if (!result.IsSuccess || result.journal == null)
                return NotFound(new { message = "Journal not found" });

            return Ok(result.journal);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJournal(int id, [FromBody] EditJournal journal)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isSuccess = await _journalService.UpdateAsync(id, journal);

            if (!isSuccess)
                return BadRequest(new { message = "Failed to update journal. Only today's journal can be edited." });

            return NoContent();
        }

        [HttpGet("can-create-today")]
        public async Task<IActionResult> CanCreateToday()
        {
            var canCreate = await _journalService.CanCreateToday();
            return Ok(new { canCreate });
        }
    }
}