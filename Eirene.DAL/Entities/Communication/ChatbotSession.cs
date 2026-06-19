namespace Eirene.DAL.Entities.Communication;

public class ChatbotSession
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastMessageAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ChatbotMessage> Messages { get; set; } = new List<ChatbotMessage>();
}
