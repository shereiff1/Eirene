using Eirene.BLL.Enumerators;
using Eirene.BLL.Models.Community.Comment;
using Eirene.BLL.Services.Abstraction.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eirene.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> GetByPostId(Guid postId)
        {
            var result = await _communityCommentServices.GetByPostIdAsync(postId);
            if (!result.IsSuccess)
                return BadRequest(new { message = "Could not retrieve comments." });
            return Ok(result.Comments);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _communityCommentServices.GetByIdAsync(id);
            if (!result.IsSuccess || result.Comment == null)
                return NotFound(new { message = "Comment not found." });
            return Ok(result.Comment);
        }

        [HttpGet("replies/{commentId}")]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> GetReplies(Guid commentId)
        {
            var result = await _communityCommentServices.GetRepliesByCommentIdAsync(commentId);
            if (!result.IsSuccess)
                return BadRequest(new { message = "Could not retrieve replies." });
            return Ok(result.Replies);
        }

        [HttpPost]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> Create([FromBody] AddCommunityComment comment)
        {
            var result = await _communityCommentServices.CreateAsync(comment);
            if (!result.IsSuccess || result.CreatedComment == null)
                return BadRequest(new { message = result.Message });
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.CreatedComment.Id },
                result.CreatedComment
            );
        }

        [HttpPut]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> Update([FromBody] EditCommunityComment comment)
        {
            var updated = await _communityCommentServices.UpdateAsync(comment);
            if (!updated)
                return NotFound(new { message = "Comment not found." });
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.AllUsers)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _communityCommentServices.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Comment not found." });
            return NoContent();
        }
    }
}
