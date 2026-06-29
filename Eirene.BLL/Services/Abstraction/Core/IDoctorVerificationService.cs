using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Admin.Verification;
using Eirene.BLL.Models.Core.Doctor.Verification;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IDoctorVerificationService
    {
        Task<Result<DoctorVerificationModel>> SubmitDoctorDocumentsAsync(string doctorId, SubmitDocumentsRequest request);
        Task<Result<bool>> DoctorUploadedDocuments(string doctorId);
        Task<Result<DoctorVerificationModel>> ReviewDoctorAsync(string adminId, string doctorId, ReviewDoctorRequest request);
        Task<Result<PagedResult<DoctorVerificationModel>>> GetPendingDoctorsAsync(int page = 1, int pageSize = 10);
        Task<Result<List<DoctorAuditLogModel>>> GetDoctorAuditLogAsync(string doctorId);
    }
}
