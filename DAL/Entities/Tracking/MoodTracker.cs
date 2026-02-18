using DAL.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;
namespace DAL.Entities.Tracking;
public class MoodTracker
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    [ForeignKey(nameof(UserId))]
    public ApplicationUser Patient { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int MoodLevel { get; set; }
    public string? Notes { get; set; }
}
