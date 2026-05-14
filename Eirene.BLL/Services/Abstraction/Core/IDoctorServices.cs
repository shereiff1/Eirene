
using Eirene.BLL.Models.Core;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.DAL.Entities.Core;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IDoctorServices : IDoctorProfileService, ISupervisionService, IDoctorRatingService
    {
    }
}
