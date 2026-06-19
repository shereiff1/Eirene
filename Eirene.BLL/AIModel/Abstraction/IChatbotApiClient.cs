namespace Eirene.BLL.AIModel.Abstraction;


public interface IChatbotApiClient
{

    Task<string?> ChatAsync(string message, List<ChatHistoryEntry> history);
}

public class ChatHistoryEntry
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
