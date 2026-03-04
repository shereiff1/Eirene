using Microsoft.AspNetCore.Http;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IPictureService
    {
        Task<(bool IsSuccess, string? Url, string? Error)> UploadPictureAsync(IFormFile file);
        Task<(bool IsSuccess, string? Error)> DeletePictureAsync(string fileUrl);
    }
}
