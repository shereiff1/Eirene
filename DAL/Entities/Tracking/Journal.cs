using DAL.Entities.Core;
namespace DAL.Entities.Tracking;
public class Journal
{
    public int Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser Patient { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Content { get; set; } = string.Empty;
}