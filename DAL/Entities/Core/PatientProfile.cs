using DAL.Entities.Tracking;

namespace DAL.Entities.Core;

public class PatientProfile
{
    public string Id { get; set; }
    public ApplicationUser User { get; set; }

    public string? CurrentTreatmentPlan { get; set; }
    public bool WantsSupervision { get; set; }
    public string? AssignedDoctorId { get; set; }
    public int DoctorRating { get; set; }
    public string? AnonymousTag { get; set; }
    public bool HasMoodTrackerEnabled { get; set; }
    public bool HighSeverityFlag { get; set; }

    // public string? HabitDataJson { get; set; } 

    public ICollection<Journal> Journals { get; set; }
}