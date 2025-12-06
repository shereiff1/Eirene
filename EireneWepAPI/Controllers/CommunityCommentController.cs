using BLL.Models.Community.Comment;
using BLL.Services.Abstraction.Community;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityCommentController : ControllerBase
    {
        private readonly ILogger<CommunityCommentController> _logger;
        private readonly ICommunityCommentServices _communityCommentServices;
        public CommunityCommentController(ILogger<CommunityCommentController> logger, ICommunityCommentServices communityCommentServices)
        {
            _logger = logger;
            _communityCommentServices = communityCommentServices;
        }
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetByPostId(int postId)
        {
            var result = await _communityCommentServices.GetByPostIdAsync(postId);
            if (!result.IsSuccess)
                return BadRequest("Could not retrieve comments.");
            return Ok(result.Comments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _communityCommentServices.GetByIdAsync(id);
            if (!result.IsSuccess || result.Comment == null)
                return NotFound();
            return Ok(result.Comment);
        }

        [HttpGet("{commentId}/replies")]
        public async Task<IActionResult> GetReplies(int commentId)
        {
            var result = await _communityCommentServices.GetRepliesByCommentIdAsync(commentId);
            if (!result.IsSuccess)
                return BadRequest("Could not retrieve replies.");
            return Ok(result.Replies);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddCommunityComment comment)
        {
            var result = await _communityCommentServices.CreateAsync(comment);
            if (!result.IsSuccess || result.CreatedComment == null)
                return BadRequest("Failed to create comment.");
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.CreatedComment.Id },
                result.CreatedComment
            );
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EditCommunityComment comment)
        {
            var updated = await _communityCommentServices.UpdateAsync(comment);
            if (!updated)
                return NotFound("Comment not found.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _communityCommentServices.DeleteAsync(id);
            if (!deleted)
                return NotFound("Comment not found.");
            return NoContent();
        }
    }
}
