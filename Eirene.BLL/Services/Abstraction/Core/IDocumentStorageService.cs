using Microsoft.AspNetCore.Http;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IDocumentStorageService
    {
        Task<(bool IsSuccess, string? Url, string? Error)> UploadDocumentAsync(IFormFile file, string doctorId);
        Task<(bool IsSuccess, string? Error)> DeleteDocumentAsync(string fileUrl);
    }
}
