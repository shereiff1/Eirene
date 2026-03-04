

namespace BLL.Models.Communication;

public class SendMessageDto
{
    public Guid ConversationId { get; set; }
    public string ReciverId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
