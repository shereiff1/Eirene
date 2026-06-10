using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eirene.DAL.Enumerators;

namespace Eirene.DAL.Entities.Core
{
    public class DoctorVerification
    {
        [Key]
        public int Id { get; set; }

        public string DoctorId { get; set; } = string.Empty;
        public DoctorProfile Doctor { get; set; } = null!;

        public string LicenseNumber { get; set; } = string.Empty;
        public string IssuingAuthority { get; set; } = string.Empty;
        public DateTime LicenseExpiryDate { get; set; }
        
        public string? SyndicateMembershipId { get; set; }
        public string? HospitalAffiliation { get; set; }

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public string? CurrentStageNote { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
