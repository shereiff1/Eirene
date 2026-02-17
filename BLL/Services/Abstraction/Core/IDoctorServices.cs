
using BLL.Models.Core.Doctor;

namespace BLL.Services.Abstraction.Core
{
    public interface IDoctorServices
    {
        Task<(bool IsSuccess, List<DoctorModel>? Doctors)> GetAllAsync();
        Task<(bool isSuccess, DoctorModel? Doctor)> GetByIdAsync(string id);
        Task<(bool IsSuccess, string? Error, DoctorModel? Doctor)> CreateDoctorProfileAsync(AddDoctorProfile model, string userId);
        Task<(bool IsSuccess, string? Error, DoctorModel? Doctor)> UpdateDoctorProfileAsync(EditDoctorProfile model, string userId);
    }
}
