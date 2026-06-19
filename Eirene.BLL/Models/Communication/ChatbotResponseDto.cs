namespace Eirene.BLL.Models.Communication;


public class ChatbotResponseDto
{
    public Guid SessionId { get; set; }
    public string Response { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
