using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities.Core
{
    public class DoctorRating
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string DoctorProfileId { get; set; } = string.Empty;
        public DoctorProfile Doctor { get; set; } = null!;

        public string PatientProfileId { get; set; } = string.Empty;
        public PatientProfile Patient { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Review { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
