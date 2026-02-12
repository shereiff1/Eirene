
using DAL.Entities.Communication;

namespace DAL.Repository.Abstraction.Communication;

public interface IChatRepository
{
    Task<Conversation> CreateConversationAsync(string doctorId, string patientId);
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId);
    Task<ChatMessage> SaveMessageAsync(Guid conversationId, string senderId, string message);
}
