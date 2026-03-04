using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eirene.DAL.Enumerators;
using Eirene.DAL.Entities.Tracking;
using Eirene.DAL.Entities.Treatment;

namespace Eirene.DAL.Entities.Core;

public class PatientProfile
{
    [Key]
    [ForeignKey(nameof(User))]
    public string Id { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;
    
    public string? DoctorProfileId { get; set; }
    public DoctorProfile? Doctor { get; set; }

    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string MedicalHistory { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }

    public ICollection<Journal> Journals { get; set; } = new List<Journal>();
    public ICollection<MoodTracker> MoodTrackers { get; set; } = new List<MoodTracker>();
    public ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
    public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
    public ICollection<SupervisionRequest> SupervisionRequests { get; set; } = new List<SupervisionRequest>();
}
