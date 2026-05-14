using Eirene.BLL.Models.Communication;
using Eirene.BLL.Services.Abstraction.Communication;
using Eirene.DAL.Entities.Communication;
using Eirene.DAL.Entities.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(IChatServices chatServices, UserManager<ApplicationUser> userManager)
        {
            _chatServices = chatServices;
            _userManager = userManager;
        }


        [HttpPost("conversations")]
        public async Task<ActionResult<Conversation>> CreateConversation([FromQuery] string to)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { message = "User not authenticated" });

            if (currentUserId == to)
                return BadRequest(new { message = "Cannot create conversation with yourself." });

            var targetUser = await _userManager.FindByIdAsync(to);
            if (targetUser == null)
                return NotFound(new { message = "Target user not found." });

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);
            var targetUserRole = targetUserRoles.FirstOrDefault();


            if (currentUserRole == "Patient" && targetUserRole != "Doctor")
                return BadRequest(new { message = "Patients can only chat with doctors." });

            if (currentUserRole == "Doctor" && targetUserRole != "Patient")
                return BadRequest(new { message = "Doctors can only chat with patients." });

            string doctorId;
            string patientId;

            if (currentUserRole == "Doctor")
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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { message = "User not authenticated" });

            var conversation = await _chatServices.GetConversationAsync(conversationId);
            if (conversation == null)
                return NotFound(new { message = "Conversation not found." });

            if (conversation.DoctorId != currentUserId && conversation.PatientId != currentUserId)
                return StatusCode(403, new { message = "Access denied" });

            var messages = await _chatServices.GetMessagesAsync(conversationId);
            return Ok(messages);
        }

        [HttpPost("messages")]
        public async Task<ActionResult<ChatMessage>> SendMessage([FromBody] SendMessageDto request)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(senderId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var message = await _chatServices.SaveMessageAsync(
                    request.ConversationId,
                    senderId,
                    request.Message);

                return Ok(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = "Access denied" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
