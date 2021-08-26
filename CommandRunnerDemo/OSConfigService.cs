using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandRunnerDemo
{
    public class OSConfigService
    {
        private const string DefaultModuleId = "osconfig";

        private readonly RegistryManager registryManager;

        public OSConfigService(RegistryManager registryManager)
        {
            this.registryManager = registryManager;
        }

        /// <summary>
        /// Get the module identity twin of a device.
        /// </summary>
        public async Task<Twin> GetTwin(string deviceId, string moduleId = DefaultModuleId)
        {
            Log.Information("Getting twin for {DeviceId}.", deviceId);

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            if (string.IsNullOrWhiteSpace(moduleId))
            {
                throw new ArgumentNullException(nameof(moduleId));
            }

            var sqlQueryString = $"SELECT * from devices.modules where deviceId = '{deviceId}' and moduleId = '{moduleId}'";
            var query = registryManager.CreateQuery(sqlQueryString);
            var result = await query.GetNextAsTwinAsync();
            return result.SingleOrDefault();
        }

        /// <summary>
        /// Get all the module identity twins.
        /// </summary>
        public async Task<List<Twin>> GetTwins(string moduleId = DefaultModuleId)
        {
            Log.Information("Getting twins.");

            if (string.IsNullOrWhiteSpace(moduleId))
            {
                throw new ArgumentNullException(nameof(moduleId));
            }

            var sqlQueryString = $"SELECT * from devices.modules where moduleId = '{moduleId}'";
            var query = registryManager.CreateQuery(sqlQueryString);

            var results = new List<Twin>();
            while (query.HasMoreResults)
            {
                var result = await query.GetNextAsTwinAsync();
                results.AddRange(result);
            }
            return results.OrderBy(x => x.DeviceId).ToList();
        }

        /// <summary>
        /// Update the CommandRunner module of the module identity twin.
        /// </summary>
        public async Task UpdateCommandRunner(Twin twin, string commandId, string arguments, int action = 3)
        {
            Log.Information("Updating twin for {DeviceId}.", twin.DeviceId);

            if (twin == null)
            {
                throw new ArgumentNullException(nameof(twin));
            }

            if (string.IsNullOrWhiteSpace(commandId))
            {
                throw new ArgumentNullException(nameof(commandId));
            }

            if (string.IsNullOrWhiteSpace(arguments))
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            var twinPatch = $@"{{
                ""properties"": {{
                    ""desired"": {{
                        ""CommandRunner"": {{
                            ""__t"": ""c"",
                            ""CommandArguments"": {{
                                ""CommandId"": ""{commandId}"",
                                ""Arguments"": ""{arguments}"",
                                ""Action"": {action}
                            }}
                        }}
                    }}
                }}
            }}";
            await registryManager.UpdateTwinAsync(twin.DeviceId, twin.ModuleId, twinPatch, twin.ETag);
        }

        /// <summary>
        /// Get the CommandRunner module of the module identity twin.
        /// </summary>
        public async Task<CommandRunner> GetCommandRunner(Twin twin)
        {
            if (twin == null)
            {
                throw new ArgumentNullException(nameof(twin));
            }

            var sqlQueryString = $"SELECT properties.reported.CommandRunner.CommandStatus FROM devices.modules where deviceId = '{twin.DeviceId}' and moduleid = 'osconfig'";
            var query = registryManager.CreateQuery(sqlQueryString);
            var result = await query.GetNextAsJsonAsync();
            return JsonConvert.DeserializeObject<CommandRunner>(result.FirstOrDefault());
        }
    }
}
