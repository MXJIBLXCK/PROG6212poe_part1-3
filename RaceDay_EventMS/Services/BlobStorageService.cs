using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay_EventMS.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;

        public BlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            var connectionString = _configuration["AzureStorage:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback for local development if Azure credentials aren't configured yet
                var localFileName = $"{Guid.NewGuid()}_{file.FileName}";
                return $"/uploads/{containerName}/{localFileName}";
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return blobClient.Uri.ToString();
        }
    }
}
