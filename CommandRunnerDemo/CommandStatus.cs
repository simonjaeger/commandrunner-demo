namespace CommandRunnerDemo
{
    public class CommandStatus
    {
        public string CommandId {  get; set; }
         
        public int ResultCode { get; set; }

        public int ExtendedResultCode {  get; set; }

        public string TextResult {  get; set; }

        public int CurrentState {  get; set; }
    }
}
