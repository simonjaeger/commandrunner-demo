using CommandLine;

namespace CommandRunnerDemo
{
    public class Arguments
    {
        [Option('d', "deviceId", Required = false, HelpText = "Identifier for the device.")]
        public string DeviceId { get; set; }

        [Option('c', "command", Required = true, HelpText = "")]
        public string Command { get; set; }

        [Option('w', "wait", Required = false, HelpText = "Wait for the command to finish.")]
        public bool Wait { get; set; }
    }
}
