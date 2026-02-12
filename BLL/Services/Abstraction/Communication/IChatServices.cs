

using DAL.Entities.Communication;

namespace BLL.Services.Abstraction.Communication;

public interface IChatServices
{
    Task<Conversation> CreateConversationAsync(string doctorId, string patientId);
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId);
    Task<ChatMessage> SaveMessageAsync(Guid conversationId, string senderId, string message);
}

