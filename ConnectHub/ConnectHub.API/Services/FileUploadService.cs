using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ConnectHub.API.Services
{
    public class FileUploadService
    {
        private readonly string _uploadDirectory;
        private readonly IWebHostEnvironment _environment;

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadDirectory = Path.Combine(_environment.ContentRootPath, "uploads");
            Directory.CreateDirectory(_uploadDirectory);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
            var filePath = Path.Combine(_uploadDirectory, uniqueFileName);

            using (var fileWriter = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileWriter);
            }

            // Return a URL that can be accessed through the API
            return $"/api/files/{uniqueFileName}";
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
            var filePath = Path.Combine(_uploadDirectory, fileName);
            
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
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
