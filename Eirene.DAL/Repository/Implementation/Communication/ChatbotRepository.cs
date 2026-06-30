using Eirene.DAL.Database;
using Eirene.DAL.Entities.Communication;
using Eirene.DAL.Repository.Abstraction.Communication;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Repository.Implementation.Communication;

internal class ChatbotRepository : IChatbotRepository
{
    private readonly EireneDBContext _dbContext;

    public ChatbotRepository(EireneDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChatbotSession> CreateSessionAsync(string userId, string? title)
    {
        var session = new ChatbotSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _dbContext.ChatbotSessions.AddAsync(session);
        return session;
    }

    public async Task<ChatbotSession?> GetSessionAsync(Guid sessionId)
    {
        return await _dbContext.ChatbotSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<List<ChatbotSession>> GetUserSessionsAsync(string userId)
    {
        return await _dbContext.ChatbotSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastMessageAt ?? s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ChatbotMessage>> GetSessionMessagesAsync(Guid sessionId)
    {
        return await _dbContext.ChatbotMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<ChatbotMessage> AddMessageAsync(Guid sessionId, string role, string content)
    {
        var message = new ChatbotMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = role,
            Content = content,
            SentAt = DateTime.UtcNow
        };

        await _dbContext.ChatbotMessages.AddAsync(message);
        return message;
    }

    public async Task UpdateSessionTimestampAsync(Guid sessionId)
    {
        var session = await _dbContext.ChatbotSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.LastMessageAt = DateTime.UtcNow;
        }
    }

    public async Task UpdateSessionTitleAsync(Guid sessionId, string title)
    {
        var session = await _dbContext.ChatbotSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Title = title;
        }
    }

    public async Task DeactivateSessionAsync(Guid sessionId)
    {
        var session = await _dbContext.ChatbotSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.IsActive = false;
        }
    }
}
