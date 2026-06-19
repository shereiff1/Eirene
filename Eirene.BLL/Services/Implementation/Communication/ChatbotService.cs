using Eirene.BLL.AIModel.Abstraction;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Communication;
using Eirene.BLL.Services.Abstraction.Communication;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Communication;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Communication;

public class ChatbotService : IChatbotService
{
    private readonly IChatbotRepository _chatbotRepository;
    private readonly IChatbotApiClient _chatbotApiClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChatbotService> _logger;

    public ChatbotService(
        IChatbotRepository chatbotRepository,
        IChatbotApiClient chatbotApiClient,
        IUnitOfWork unitOfWork,
        ILogger<ChatbotService> logger)
    {
        _chatbotRepository = chatbotRepository;
        _chatbotApiClient = chatbotApiClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ChatbotResponseDto>> SendMessageAsync(string userId, ChatbotSendMessageDto request)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<ChatbotResponseDto>("User ID is required.");

        if (string.IsNullOrWhiteSpace(request.Message))
            return Result.Failure<ChatbotResponseDto>("Message cannot be empty.");

        Guid sessionId;
        if (request.SessionId.HasValue && request.SessionId.Value != Guid.Empty)
        {
            var session = await _chatbotRepository.GetSessionAsync(request.SessionId.Value);
            if (session == null)
                return Result.Failure<ChatbotResponseDto>("Session not found.");

            if (session.UserId != userId)
                return Result.Failure<ChatbotResponseDto>("Access denied to this session.");

            if (!session.IsActive)
                return Result.Failure<ChatbotResponseDto>("This session has been closed.");

            sessionId = session.Id;
        }
        else
        {
            var newSession = await _chatbotRepository.CreateSessionAsync(userId, null);
            await _unitOfWork.SaveChangesAsync();
            sessionId = newSession.Id;
        }

        var existingMessages = await _chatbotRepository.GetSessionMessagesAsync(sessionId);
        var history = existingMessages.Select(m => new ChatHistoryEntry
        {
            Role = m.Role,
            Content = m.Content
        }).ToList();
        var chatbotResponse = await _chatbotApiClient.ChatAsync(request.Message, history);

        if (string.IsNullOrEmpty(chatbotResponse))
        {
            _logger.LogWarning("Chatbot service returned empty response for session {SessionId}", sessionId);
            return Result.Failure<ChatbotResponseDto>(
                "The chatbot is currently busy or unavailable. Please try again in a moment.");
        }

        await _chatbotRepository.AddMessageAsync(sessionId, "user", request.Message);

        await _chatbotRepository.AddMessageAsync(sessionId, "assistant", chatbotResponse);

        await _chatbotRepository.UpdateSessionTimestampAsync(sessionId);

        if (existingMessages.Count == 0)
        {
            var title = request.Message.Length > 50
                ? request.Message[..50] + "..."
                : request.Message;
            await _chatbotRepository.UpdateSessionTitleAsync(sessionId, title);
        }

        await _unitOfWork.SaveChangesAsync();

        return Result.Success(new ChatbotResponseDto
        {
            SessionId = sessionId,
            Response = chatbotResponse,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task<Result<List<ChatbotSessionDto>>> GetUserSessionsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<List<ChatbotSessionDto>>("User ID is required.");

        var sessions = await _chatbotRepository.GetUserSessionsAsync(userId);

        var dtos = sessions.Select(s => new ChatbotSessionDto
        {
            Id = s.Id,
            Title = s.Title,
            CreatedAt = s.CreatedAt,
            LastMessageAt = s.LastMessageAt,
            IsActive = s.IsActive
        }).ToList();

        return Result.Success(dtos);
    }

    public async Task<Result<List<ChatbotMessageDto>>> GetSessionMessagesAsync(string userId, Guid sessionId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<List<ChatbotMessageDto>>("User ID is required.");

        var session = await _chatbotRepository.GetSessionAsync(sessionId);
        if (session == null)
            return Result.Failure<List<ChatbotMessageDto>>("Session not found.");

        if (session.UserId != userId)
            return Result.Failure<List<ChatbotMessageDto>>("Access denied to this session.");

        var messages = await _chatbotRepository.GetSessionMessagesAsync(sessionId);

        var dtos = messages.Select(m => new ChatbotMessageDto
        {
            Id = m.Id,
            Role = m.Role,
            Content = m.Content,
            SentAt = m.SentAt
        }).ToList();

        return Result.Success(dtos);
    }

    public async Task<Result> DeleteSessionAsync(string userId, Guid sessionId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure("User ID is required.");

        var session = await _chatbotRepository.GetSessionAsync(sessionId);
        if (session == null)
            return Result.Failure("Session not found.");

        if (session.UserId != userId)
            return Result.Failure("Access denied to this session.");

        await _chatbotRepository.DeactivateSessionAsync(sessionId);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
