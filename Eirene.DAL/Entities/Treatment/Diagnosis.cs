using Eirene.DAL.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eirene.DAL.Entities.Treatment;

public class Diagnosis
{
    public Guid Id { get; set; }
    public string DiagnosisName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    [ForeignKey(nameof(PatientId))]
    public ApplicationUser Patient { get; set; } = null!;

}