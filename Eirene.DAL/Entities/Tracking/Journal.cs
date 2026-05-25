using Eirene.DAL.Entities.Core;

namespace Eirene.DAL.Entities.Tracking;

public class Journal
{
    public Guid Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser Patient { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Content { get; set; } = string.Empty;
    public float Mood { get; set; } = 1;
}
