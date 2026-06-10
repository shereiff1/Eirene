using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eirene.DAL.Entities.Core
{
    public class DoctorAuditLog
    {
        [Key]
        public int Id { get; set; }

        public string DoctorId { get; set; } = string.Empty;
        public DoctorProfile Doctor { get; set; } = null!;

        public string AdminId { get; set; } = string.Empty;
        public ApplicationUser Admin { get; set; } = null!;

        public string Action { get; set; } = string.Empty;
        public string? Reason { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
