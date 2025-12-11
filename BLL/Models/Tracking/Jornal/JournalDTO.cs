namespace BLL.Models.Tracking;

public class JournalDTO
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal Mood { get; set; }
}