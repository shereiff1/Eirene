using Eirene.DAL.Enumerators;

namespace Eirene.DAL.Entities.Core;

public class SupervisionRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string PatientProfileId { get; set; } = string.Empty;
    public PatientProfile Patient { get; set; } = null!;

    public string DoctorProfileId { get; set; } = string.Empty;
    public DoctorProfile Doctor { get; set; } = null!;

    public SupervisionRequestStatus Status { get; set; } = SupervisionRequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    public void Accept()
    {
        if (Status != SupervisionRequestStatus.Pending)
            throw new InvalidOperationException("This request has already been responded to.");

        Status = SupervisionRequestStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
    }

    public void Decline()
    {
        if (Status != SupervisionRequestStatus.Pending)
            throw new InvalidOperationException("This request has already been responded to.");

        Status = SupervisionRequestStatus.Declined;
        RespondedAt = DateTime.UtcNow;
    }
}
