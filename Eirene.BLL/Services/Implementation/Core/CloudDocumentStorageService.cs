using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Eirene.BLL.Services.Abstraction.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class CloudDocumentStorageService : IDocumentStorageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudDocumentStorageService> _logger;

        public CloudDocumentStorageService(IConfiguration config, ILogger<CloudDocumentStorageService> logger)
        {
            _logger = logger;
            var cloudName = config["Storage:CloudinarySettings:CloudName"];
            var apiKey = config["Storage:CloudinarySettings:ApiKey"];
            var apiSecret = config["Storage:CloudinarySettings:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                _logger.LogError("Cloudinary settings are missing or invalid in appsettings.json.");
                throw new ArgumentException("Cloudinary settings are missing or invalid.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
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
                await using var stream = file.OpenReadStream();
                
                // If it's a PDF, we should upload it as raw or image. Cloudinary allows image/raw. 
                // Using RawUploadParams for PDFs and ImageUploadParams for images.
                if (extension == ".pdf")
                {
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = $"doctors/{doctorId}/documents"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    if (uploadResult.Error != null)
                        return (false, null, uploadResult.Error.Message);

                    return (true, uploadResult.SecureUrl.ToString(), null);
                }
                else
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = $"doctors/{doctorId}/documents"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    if (uploadResult.Error != null)
                        return (false, null, uploadResult.Error.Message);

                    return (true, uploadResult.SecureUrl.ToString(), null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document to Cloudinary");
                return (false, null, "An error occurred while uploading the document.");
            }
        }

        public async Task<(bool IsSuccess, string? Error)> DeleteDocumentAsync(string fileUrl)
        {
            try
            {
                var uri = new Uri(fileUrl);
                var segments = uri.Segments;
                var publicIdWithExtension = segments.Last();
                var folderPath = string.Join("", segments.Skip(Array.IndexOf(segments, "doctors/") - 1).Take(segments.Length - Array.IndexOf(segments, "doctors/") - 1 + 1));
                
                var publicId = folderPath + Path.GetFileNameWithoutExtension(publicIdWithExtension);

                // Cloudinary deletion requires resource type for raw files
                if (fileUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Raw };
                    var result = await _cloudinary.DestroyAsync(deletionParams);
                    if (result.Result == "ok") return (true, null);
                }
                else
                {
                    var deletionParams = new DeletionParams(publicId);
                    var result = await _cloudinary.DestroyAsync(deletionParams);
                    if (result.Result == "ok") return (true, null);
                }

                return (false, "Failed to delete from Cloudinary");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document from Cloudinary");
                return (false, "An error occurred while deleting the document.");
            }
        }
    }
}
