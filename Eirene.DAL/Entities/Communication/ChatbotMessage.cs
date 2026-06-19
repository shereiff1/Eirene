namespace Eirene.DAL.Entities.Communication;


public class ChatbotMessage
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public ChatbotSession Session { get; set; } = null!;
}
