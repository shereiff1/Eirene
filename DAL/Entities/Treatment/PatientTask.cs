using DAL.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities.Treatment;

public class PatientTask
{
    public int Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public int TreatmentPlanId { get; set; }
    public TreatmentPlan TreatmentPlan { get; set; } = null!;


    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
