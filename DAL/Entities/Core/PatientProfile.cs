using DAL.Entities.Tracking;

namespace DAL.Entities.Core
{
    public class PatientProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string MedicalHistory { get; set; } = string.Empty;

        public ICollection<Journal> Journals { get; set; } = new List<Journal>();
    }
}
