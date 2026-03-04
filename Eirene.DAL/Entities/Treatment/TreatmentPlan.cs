using DAL.Entities.Core;

namespace DAL.Entities.Treatment;

public class TreatmentPlan
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<PatientTask> Tasks { get; set; } = new List<PatientTask>();
}
