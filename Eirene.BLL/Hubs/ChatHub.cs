
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
