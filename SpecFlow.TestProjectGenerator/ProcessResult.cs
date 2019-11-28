namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class ProcessResult
    {
        public ProcessResult(int exitCode, string stdOutput, string stdError, string combinedOutput)
        {
            ExitCode = exitCode;
            StdOutput = stdOutput;
            StdError = stdError;
            CombinedOutput = combinedOutput;
        }

        public string StdOutput { get; }
        public string StdError { get; }
        public string CombinedOutput { get; }
        public int ExitCode { get; }
    }
}
