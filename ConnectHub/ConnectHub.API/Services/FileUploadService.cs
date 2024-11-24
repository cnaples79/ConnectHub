using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ConnectHub.API.Services
{
    public class FileUploadService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public FileUploadService(IConfiguration configuration)
        {
            _blobServiceClient = new BlobServiceClient(configuration["AzureStorage:ConnectionString"]);
            _containerName = configuration["AzureStorage:ContainerName"];
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
            var blobClient = container.GetBlobClient(uniqueFileName);

            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var uri = new Uri(fileUrl);
            var blobName = uri.Segments.Last();
            var blobClient = container.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<bool> ValidateFileAsync(Stream fileStream, string contentType, long maxSizeInBytes = 5242880)
        {
            if (fileStream.Length > maxSizeInBytes) // 5MB default limit
                return false;

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            return allowedTypes.Contains(contentType.ToLower());
        }
    }
}
