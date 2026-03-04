namespace Eirene.BLL.Models.Core.Patient;

public class PatientModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string MedicalHistory { get; set; } = string.Empty;
    public DateTime dateOfBirth { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}