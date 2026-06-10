using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eirene.DAL.Enumerators;

namespace Eirene.DAL.Entities.Core
{
    public class DoctorDocument
    {
        [Key]
        public int Id { get; set; }

        public string DoctorId { get; set; } = string.Empty;
        public DoctorProfile Doctor { get; set; } = null!;

        public DocumentType DocumentType { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DocumentReviewStatus ReviewStatus { get; set; } = DocumentReviewStatus.Pending;
        public string? AdminNotes { get; set; }
    }
}
