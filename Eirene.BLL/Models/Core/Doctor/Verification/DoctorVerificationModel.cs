using Eirene.DAL.Enumerators;

namespace Eirene.BLL.Models.Core.Doctor.Verification
{
    public class DoctorVerificationModel
    {
        public int Id { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string IssuingAuthority { get; set; } = string.Empty;
        public DateTime LicenseExpiryDate { get; set; }
        public string? SyndicateMembershipId { get; set; }
        public string? HospitalAffiliation { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public string? CurrentStageNote { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        // We can include documents for the query "GetPendingDoctorsQuery ... Include their submitted documents list"
        public List<DoctorDocumentModel> Documents { get; set; } = new List<DoctorDocumentModel>();
    }

    public class DoctorDocumentModel
    {
        public int Id { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public DocumentReviewStatus ReviewStatus { get; set; }
        public string? AdminNotes { get; set; }
    }
}
