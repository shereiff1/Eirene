using Eirene.BLL.Services.Abstraction.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class LocalPictureService : IPictureService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalPictureService> _logger;

        public LocalPictureService(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ILogger<LocalPictureService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool IsSuccess, string? Url, string? Error)> UploadPictureAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, null, "No file uploaded.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    return (false, null, "Invalid file type. Only jpg, jpeg, png, gif are allowed.");

                // Max file size 5MB
                if (file.Length > 5 * 1024 * 1024)
                    return (false, null, "File size exceeds the 5MB limit.");

                var profilesPath = _configuration["Storage:Local:ProfilesPath"] ?? "images/profiles";
                var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, profilesPath.Replace("/", "\\"));

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var url = $"/{profilesPath}/{uniqueFileName}";
                return (true, url, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading local picture.");
                return (false, null, "An error occurred while uploading the picture.");
            }
        }

        public Task<(bool IsSuccess, string? Error)> DeletePictureAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return Task.FromResult<(bool, string?)>((false, "File URL is empty."));

                // Ensure fileUrl starts with a forward slash to match local routing
                if (fileUrl.StartsWith("/"))
                {
                    fileUrl = fileUrl.Substring(1);
                }

                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, fileUrl.Replace("/", "\\"));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return Task.FromResult<(bool, string?)>((true, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting local picture.");
                return Task.FromResult<(bool, string?)>((false, "An error occurred while deleting the picture."));
            }
        }
    }
}
