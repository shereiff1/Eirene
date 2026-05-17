using Eirene.DAL.Enumerators;

namespace Eirene.BLL.Models.Core.Doctor;

public class SupervisionRequestDTO
{
    public string Id { get; set; } = string.Empty;
    public string PatientProfileId { get; set; } = string.Empty;
    public string PatientFullName { get; set; } = string.Empty;
    public string? PatientProfilePhotoUrl { get; set; }
    public SupervisionRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
