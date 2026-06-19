using Eirene.DAL.Entities.Communication;

namespace Eirene.DAL.Repository.Abstraction.Communication;

public interface IChatbotRepository
{
    Task<ChatbotSession> CreateSessionAsync(string userId, string? title);
    Task<ChatbotSession?> GetSessionAsync(Guid sessionId);
    Task<List<ChatbotSession>> GetUserSessionsAsync(string userId);
    Task<List<ChatbotMessage>> GetSessionMessagesAsync(Guid sessionId);
    Task<ChatbotMessage> AddMessageAsync(Guid sessionId, string role, string content);
    Task UpdateSessionTimestampAsync(Guid sessionId);
    Task UpdateSessionTitleAsync(Guid sessionId, string title);
    Task DeactivateSessionAsync(Guid sessionId);
}
