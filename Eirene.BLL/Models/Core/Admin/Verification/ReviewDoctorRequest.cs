using Eirene.DAL.Enumerators;

namespace Eirene.BLL.Models.Core.Admin.Verification
{
    public class ReviewDoctorRequest
    {
        public VerificationStatus NewStatus { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
