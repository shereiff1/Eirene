using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities.Core
{
    public class DoctorProfile
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public string Biography { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Qualifications { get; set; } = string.Empty;
        public double Rating { get; set; } = 0.0;
        public int ReviewCount { get; set; } = 0;
        public ICollection<PatientProfile> Patients { get; set; } = new List<PatientProfile>();
        public string? ProfilePhotoUrl { get; set; }
        public bool isActive { get; set; } = true;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
    }
}
