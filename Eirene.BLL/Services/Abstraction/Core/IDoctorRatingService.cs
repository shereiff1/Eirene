using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IDoctorRatingService
    {
        Task<Result<List<DoctorRatingDTO>>> GetDoctorRatingsAsync(string doctorId);
    }
}
