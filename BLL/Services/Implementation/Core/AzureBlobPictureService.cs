using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BLL.Services.Abstraction.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Core
{
    public class AzureBlobPictureService : IPictureService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobPictureService> _logger;

        public AzureBlobPictureService(IConfiguration configuration, ILogger<AzureBlobPictureService> logger)
        {
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

                if (file.Length > 5 * 1024 * 1024)
                    return (false, null, "File size exceeds the 5MB limit.");

                var connectionString = _configuration["Storage:Azure:ConnectionString"];
                var containerName = _configuration["Storage:Azure:ContainerName"] ?? "profile-pictures";

                if (string.IsNullOrEmpty(connectionString))
                    return (false, null, "Azure Storage Connection String is not configured.");

                var blobServiceClient = new BlobServiceClient(connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var uniqueFileName = Guid.NewGuid().ToString() + ext;
                var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
                }

                return (true, blobClient.Uri.ToString(), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading picture to Azure Blob Storage.");
                return (false, null, "An error occurred while uploading the picture.");
            }
        }

        public async Task<(bool IsSuccess, string? Error)> DeletePictureAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return (false, "File URL is empty.");

                var connectionString = _configuration["Storage:Azure:ConnectionString"];
                var containerName = _configuration["Storage:Azure:ContainerName"] ?? "profile-pictures";

                if (string.IsNullOrEmpty(connectionString))
                    return (false, "Azure Storage Connection String is not configured.");

                var uri = new Uri(fileUrl);
                var fileName = Path.GetFileName(uri.LocalPath);

                var blobServiceClient = new BlobServiceClient(connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(fileName);

                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting picture from Azure Blob Storage.");
                return (false, "An error occurred while deleting the picture.");
            }
        }
    }
}
