using Azure.Storage.Blobs;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CommandRunnerDemo
{
    public class StorageService
    {
        private const string DefaultContainerName = "osconfig";

        private readonly BlobServiceClient blobServiceClient;

        public StorageService(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        /// <summary>
        /// Upload a file to the Azure Blob Storage container.
        /// </summary>
        public async Task<Uri> UploadFile(string prefix, string file, string containerName = DefaultContainerName)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                Log.Error("File to run does not exist.");
                return null;
            }

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            using var fileStream = File.OpenRead(file);
            var blobName = $"{prefix}/{file.Split('/', '\\').Last()}";
            var response = await blobContainerClient.UploadBlobAsync(blobName, fileStream);

            var blobClient = blobContainerClient.GetBlobClient(blobName);
            var uri = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTime.UtcNow.AddYears(1));
            return uri;
        }
    }
}
