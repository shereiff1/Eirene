using Eirene.BLL.Models.Communication;
using Eirene.BLL.Services.Abstraction.Communication;
using Eirene.DAL.Entities.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eirene.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatServices _chatServices;

        public ChatController(IChatServices chatServices)
        {
            _chatServices = chatServices;
        }


        [HttpPost("conversations")]
        public async Task<ActionResult<Conversation>> CreateConversation([FromQuery] string to)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            if (currentUserId == to)
                return BadRequest("Cannot create conversation with yourself.");

            var role = User.FindFirstValue(ClaimTypes.Role);

            string doctorId;
            string patientId;

            if (role == "Doctor")
            {
                doctorId = currentUserId;
                patientId = to;
            }
            else
            {
                doctorId = to;
                patientId = currentUserId;
            }

            var conversation = await _chatServices.CreateConversationAsync(doctorId, patientId);

            return Ok(conversation);
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages(Guid conversationId)
        {
            var messages = await _chatServices.GetMessagesAsync(conversationId);
            return Ok(messages);
        }

        [HttpPost("messages")]
        public async Task<ActionResult<ChatMessage>> SendMessage([FromBody] SendMessageDto request)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(senderId))
                return Unauthorized();

            var message = await _chatServices.SaveMessageAsync(
                request.ConversationId,
                senderId,
                request.Message);

            return Ok(message);
        }
    }
}
