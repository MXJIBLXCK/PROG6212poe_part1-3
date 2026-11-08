using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RaceDay_EventMS.Services
{
    public class BlobStorageService(IConfiguration configuration, IWebHostEnvironment env) : IBlobStorageService
    {
        private readonly string _connectionString = configuration["AzureStorage:ConnectionString"] ?? string.Empty;
        private readonly IWebHostEnvironment _env = env;

        public async Task<string> UploadFileAsync(IFormFile file, string containerName = "event-banners")
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            // Fallback to local wwwroot/uploads if connection string is empty or set to dev storage
            if (string.IsNullOrEmpty(_connectionString) || _connectionString == "UseDevelopmentStorage=true")
            {
                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/{uniqueFileName}";
            }

            // Real Azure Blob Storage execution
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }
    }
}
