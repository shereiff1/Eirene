namespace Eirene.BLL.Models.Core.Doctor;

public class DoctorPatientDTO
{
    public string RequestId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime AcceptedAt { get; set; }
}
