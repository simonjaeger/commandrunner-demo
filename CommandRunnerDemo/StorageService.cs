using Azure.Storage.Blobs;
using Azure.Storage.Sas;
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
            var blobSasBuilder = new BlobSasBuilder(BlobContainerSasPermissions.Read, DateTime.UtcNow.AddYears(1));
            blobSasBuilder.Protocol = SasProtocol.HttpsAndHttp;
            var uri = blobClient.GenerateSasUri(blobSasBuilder);
            // NOTE: Support for MCC.
            var newUri = new Uri(uri.AbsoluteUri.Replace("https://", "http://"));
            return newUri;
        }
    }
}
