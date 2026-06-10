using Eirene.BLL.Services.Abstraction.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class LocalDocumentStorageService : IDocumentStorageService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LocalDocumentStorageService> _logger;

        public LocalDocumentStorageService(IConfiguration config, ILogger<LocalDocumentStorageService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<(bool IsSuccess, string? Url, string? Error)> UploadDocumentAsync(IFormFile file, string doctorId)
        {
            if (file == null || file.Length == 0)
                return (false, null, "File is empty.");

            if (file.Length > 5 * 1024 * 1024)
                return (false, null, "File size exceeds 5MB limit.");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".pdf" && extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                return (false, null, "Only PDF, JPG, and PNG files are allowed.");

            try
            {
                var folderName = Path.Combine("wwwroot", "uploads", "doctors", doctorId);
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = Path.Combine("uploads", "doctors", doctorId, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return (true, dbPath, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document to local storage");
                return (false, null, "An error occurred while uploading the document.");
            }
        }

        public Task<(bool IsSuccess, string? Error)> DeleteDocumentAsync(string fileUrl)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileUrl);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult((true, (string?)null));
                }
                return Task.FromResult((false, (string?)"File not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document from local storage");
                return Task.FromResult((false, (string?)"An error occurred while deleting the document."));
            }
        }
    }
}
