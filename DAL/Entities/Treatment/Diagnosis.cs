using DAL.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities.Treatment;

public class Diagnosis
{
    public int Id { get; set; }
    public string DiagnosisName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    [ForeignKey(nameof(PatientId))]
    public ApplicationUser Patient { get; set; } = null!;

}