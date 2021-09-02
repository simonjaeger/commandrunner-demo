using Azure.Storage.Blobs;
using CommandLine;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandRunnerDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Arguments arguments = null;
            Parser.Default.ParseArguments<Arguments>(args).WithParsed(a => arguments = a);

            if (arguments == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(arguments.Command) && !string.IsNullOrWhiteSpace(arguments.File))
            {
                Log.Error("Missing command or file to run.");
                return;
            }

            Log.Information("Initializing.");
            var iotHubConnectionString = Environment.GetEnvironmentVariable("IOT_HUB_CONNECTION_STRING");
            var blobStorageConnectionString = Environment.GetEnvironmentVariable("BLOB_STORAGE_CONNECTION_STRING");
            var registryManager =  RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            var blobServiceClient = new BlobServiceClient(blobStorageConnectionString);
            var service = new OSConfigService(registryManager);
            var storageService = new StorageService(blobServiceClient);

            var commandId = Guid.NewGuid().ToString();
            var command = arguments.Command;

            if (!string.IsNullOrWhiteSpace(arguments.File))
            {
                Log.Information("Uploading file.");
                var uri = await storageService.UploadFile(commandId, arguments.File);
                command = CommandTemplate.Create(commandId, arguments.File.Split('/', '\\').Last(), uri, arguments.UseDeliveryOptimization);
            }

            Queue<Twin> queue = new();

            if (!string.IsNullOrWhiteSpace(arguments.DeviceId))
            {
                var twin = await service.GetTwin(arguments.DeviceId);
                await service.UpdateCommandRunner(twin, commandId, command);
                queue.Enqueue(twin);
            }
            else
            {
                var twins = await service.GetTwins();
                foreach (var twin in twins)
                {
                    await service.UpdateCommandRunner(twin, commandId, command);
                    queue.Enqueue(twin);
                }
            }

            if (arguments.Wait)
            {
                Log.Information("Waiting.");

                // TODO: Implement timeout.

                while (queue.Any())
                {
                    await Task.Delay(2500);

                    var twin = queue.Dequeue();
                    var commandRunner = await service.GetCommandRunner(twin);
                    var commandStatus = commandRunner?.CommandStatus;
                    if (commandStatus?.CommandId != commandId)
                    {
                        // Put it back in the queue and wait a bit more.
                        queue.Enqueue(twin);
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(commandStatus.TextResult))
                    {
                        Log.Information($@"Updated twin for {{DeviceId}} reported ""{{@TextResult}}"".", twin.DeviceId, commandStatus.TextResult.Trim());
                    }
                }
            }

            Log.Information("Finished.");
        }
    }
}
