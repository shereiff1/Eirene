using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Entities.Tracking;
using DAL.Entities.Treatment;

namespace DAL.Entities.Core;

public class PatientProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    // [Key]
    // [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;
    
    public string? DoctorProfileId { get; set; }
    public DoctorProfile? Doctor { get; set; }

    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string MedicalHistory { get; set; } = string.Empty;

    public ICollection<Journal> Journals { get; set; } = new List<Journal>();
    public ICollection<MoodTracker> MoodTrackers { get; set; } = new List<MoodTracker>();
    public ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
    public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
}
