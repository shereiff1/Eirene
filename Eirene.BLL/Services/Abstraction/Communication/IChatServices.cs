

using Eirene.DAL.Entities.Communication;

namespace Eirene.BLL.Services.Abstraction.Communication;

public interface IChatServices
{
    Task<Conversation> CreateConversationAsync(string doctorId, string patientId);
    Task<Conversation?> GetConversationAsync(Guid conversationId);
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId);
    Task<ChatMessage> SaveMessageAsync(Guid conversationId, string senderId, string message);
}
