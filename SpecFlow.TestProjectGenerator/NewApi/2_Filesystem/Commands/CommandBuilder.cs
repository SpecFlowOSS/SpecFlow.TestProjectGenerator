using System;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands
{
    public class CommandBuilder
    {
        public CommandBuilder(string executablePath, string argumentsFormat)
        {
            ExecutablePath = executablePath;
            ArgumentsFormat = argumentsFormat;
        }

        public string ArgumentsFormat { get; }
        public string ExecutablePath { get; }

        public CommandResult Execute()
        {
            var solutionCreateProcessHelper = new ProcessHelper();

            var processResult = solutionCreateProcessHelper.RunProcess(".", ExecutablePath, ArgumentsFormat);
            if (processResult.ExitCode > 0)
            {
                var innerException = new Exception(processResult.CombinedOutput);
                throw new Exception($"Error while executing {ExecutablePath} {ArgumentsFormat}", innerException);
            }

            return new CommandResult(processResult.ExitCode, processResult.CombinedOutput);
        }
    }
}
