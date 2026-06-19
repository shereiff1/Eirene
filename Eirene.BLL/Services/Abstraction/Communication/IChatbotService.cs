using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Communication;

namespace Eirene.BLL.Services.Abstraction.Communication;

public interface IChatbotService
{
    Task<Result<ChatbotResponseDto>> SendMessageAsync(string userId, ChatbotSendMessageDto request);

    Task<Result<List<ChatbotSessionDto>>> GetUserSessionsAsync(string userId);
    Task<Result<List<ChatbotMessageDto>>> GetSessionMessagesAsync(string userId, Guid sessionId);
    Task<Result> DeleteSessionAsync(string userId, Guid sessionId);
}
