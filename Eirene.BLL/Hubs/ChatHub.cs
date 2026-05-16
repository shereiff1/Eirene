
using Eirene.BLL.Services.Abstraction.Communication;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Eirene.BLL.Hubs;

public class ChatHub : Hub
{
    private readonly IChatServices _chatServices;

    public ChatHub(IChatServices chatServices)
    {
        _chatServices = chatServices;
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            throw new HubException("Unauthorized");

        var conversation = await _chatServices.GetConversationAsync(conversationId);
        if (conversation == null)
            throw new HubException("Conversation not found.");

        if (conversation.DoctorId != userId && conversation.PatientId != userId)
            throw new HubException("You are not a participant in this conversation.");

        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task SendMessage(Guid conversationId, string message)
    {
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (senderId == null)
            throw new HubException("Unauthorized");

        var savedMessage = await _chatServices.SaveMessageAsync(conversationId, senderId, message);

        await Clients.Group(conversationId.ToString())
            .SendAsync("ReceiveMessage", savedMessage);
    }
}
