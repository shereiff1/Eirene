using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eirene.DAL.Enumerators;

namespace Eirene.DAL.Entities.Core
{
    public class DoctorProfile
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string Id { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public string Biography { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Qualifications { get; set; } = string.Empty;
        public double Rating { get; set; } = 0.0;
        public int ReviewCount { get; set; } = 0;
        public ICollection<PatientProfile> Patients { get; set; } = new List<PatientProfile>();
        public ICollection<SupervisionRequest> SupervisionRequests { get; set; } = new List<SupervisionRequest>();
        public ICollection<DoctorRating> DoctorRatings { get; set; } = new List<DoctorRating>();
        public DoctorVerification? DoctorVerification { get; set; }
        public ICollection<DoctorDocument> DoctorDocuments { get; set; } = new List<DoctorDocument>();
        public ICollection<DoctorAuditLog> DoctorAuditLogs { get; set; } = new List<DoctorAuditLog>();
        public string? ProfilePhotoUrl { get; set; }
        public bool isActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public void Verify()
        {
            if (IsVerified)
                throw new InvalidOperationException("Doctor is already verified.");

            IsVerified = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
