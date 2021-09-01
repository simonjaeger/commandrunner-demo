using CommandLine;

namespace CommandRunnerDemo
{
    public class Arguments
    {
        [Option('d', "deviceId", Required = false, HelpText = "Identifier for the device.")]
        public string DeviceId { get; set; }

        [Option('c', "command", Required = false, HelpText = "Command to run.")]
        public string Command { get; set; }

        [Option('f', "file", Required = false, HelpText = "File to run.")]
        public string File { get; set; }

        [Option('d', "deliveryoptimization", Required = false, HelpText = "Use delivery optimization to download the file.")]
        public bool UseDeliveryOptimization { get; set; }

        [Option('w', "wait", Required = false, HelpText = "Wait for the command to finish.")]
        public bool Wait { get; set; }
    }
}
