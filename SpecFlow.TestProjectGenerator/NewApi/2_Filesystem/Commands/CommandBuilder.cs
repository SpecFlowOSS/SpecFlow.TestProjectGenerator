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

            int exitCode = solutionCreateProcessHelper.RunProcess(".", ExecutablePath, ArgumentsFormat);
            if (exitCode > 0)
            {
                var innerException = new Exception(solutionCreateProcessHelper.ConsoleOutput);
                throw new Exception($"Error while executing {ExecutablePath} {ArgumentsFormat}", innerException);
            }

            return new CommandResult(exitCode, solutionCreateProcessHelper.ConsoleOutput);
        }
    }
}
