namespace Eirene.BLL.Models.Communication;

public class ChatbotSessionDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public bool IsActive { get; set; }
}
