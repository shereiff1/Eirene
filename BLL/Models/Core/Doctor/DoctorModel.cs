namespace BLL.Models.Core.Doctor;

public class DoctorModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Qualifications { get; set; } = string.Empty;
    public double Rating { get; set; } = 0.0;
    public int ReviewCount { get; set; } = 0;
    public int PatientCount { get; set; } = 0;
    public string? ProfilePhotoUrl { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string ExperienceLevel => YearsOfExperience switch
    {
        < 2 => "Junior",
        < 5 => "Mid-Level",
        < 10 => "Senior",
        _ => "Expert"
    };
}