using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.Models.Communication;

public class ChatbotSendMessageDto
{
    public Guid? SessionId { get; set; }
    [Required]
    [MinLength(1)]
    public string Message { get; set; } = string.Empty;
}
