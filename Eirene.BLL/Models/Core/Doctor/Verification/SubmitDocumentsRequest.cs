using Eirene.DAL.Enumerators;
using Microsoft.AspNetCore.Http;

namespace Eirene.BLL.Models.Core.Doctor.Verification
{
    public class SubmitDocumentsRequest
    {
        public string LicenseNumber { get; set; } = string.Empty;
        public string IssuingAuthority { get; set; } = string.Empty;
        public DateTime LicenseExpiryDate { get; set; }
        
        public string? SyndicateMembershipId { get; set; }
        public string? HospitalAffiliation { get; set; }

        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
        public List<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
    }
}
