using Eirene.BLL.Services.Abstraction.Core;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class CloudImageStorage : IPictureService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudImageStorage> _logger;

        public CloudImageStorage(
            IConfiguration configuration,
            ILogger<CloudImageStorage> logger)
        {
            _logger = logger;

            var settings = configuration.GetSection("CloudinarySettings");

            var account = new Account(
                settings["CloudName"],
                settings["ApiKey"],
                settings["ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
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

                if (file.Length > 5 * 1024 * 1024)
                    return (false, null, "File size exceeds the 5MB limit.");

                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "profile-pictures",
                    UseFilename = false,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    return (false, null, uploadResult.Error?.Message ?? "Upload failed.");

                return (true, uploadResult.SecureUrl.ToString(), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading picture to Cloudinary.");
                return (false, null, "An error occurred while uploading the picture.");
            }
        }

        public async Task<(bool IsSuccess, string? Error)> DeletePictureAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return (false, "File URL is empty.");

                var uri = new Uri(fileUrl);

                var segments = uri.AbsolutePath.Split("/upload/");
                if (segments.Length < 2)
                    return (false, "Invalid Cloudinary URL.");

                var publicIdWithExtension = segments[1];

                var publicId = Path.ChangeExtension(publicIdWithExtension, null);

                var deletionParams = new DeletionParams(publicId);

                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result != "ok")
                    return (false, "Failed to delete image.");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting picture from Cloudinary.");
                return (false, "An error occurred while deleting the picture.");
            }
        }
    }
}