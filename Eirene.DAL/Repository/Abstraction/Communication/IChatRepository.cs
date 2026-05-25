using Eirene.DAL.Entities.Communication;

namespace Eirene.DAL.Repository.Abstraction.Communication;

public interface IChatRepository
{
    Task<Conversation> CreateConversationAsync(string doctorId, string patientId);
    Task<Conversation?> GetConversationAsync(Guid conversationId);
    Task<List<ChatMessage>> GetMessagesAsync(Guid conversationId);
    Task<ChatMessage> SaveMessageAsync(Guid conversationId, string senderId, string message);
}
