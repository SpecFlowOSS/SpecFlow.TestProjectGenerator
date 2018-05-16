using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands
{
    public class CommandBuilder
    {
        private readonly IOutputWriter _outputWriter;

        public CommandBuilder(IOutputWriter outputWriter, string executablePath, string argumentsFormat)
        {
            _outputWriter = outputWriter;
            ExecutablePath = executablePath;
            ArgumentsFormat = argumentsFormat;
        }

        public string ArgumentsFormat { get; }
        public string ExecutablePath { get; }

        public CommandResult Execute()
        {
            var solutionCreateProcessHelper = new ProcessHelper();

            var processResult = solutionCreateProcessHelper.RunProcess(_outputWriter, ".", ExecutablePath, ArgumentsFormat);
            if (processResult.ExitCode > 0)
            {
                var innerException = new Exception(processResult.CombinedOutput);
                throw new Exception($"Error while executing {ExecutablePath} {ArgumentsFormat}", innerException);
            }

            return new CommandResult(processResult.ExitCode, processResult.CombinedOutput);
        }

        public CommandResult Execute(Exception ex)
        {
            var solutionCreateProcessHelper = new ProcessHelper();

            var processResult = solutionCreateProcessHelper.RunProcess(_outputWriter, ".", ExecutablePath, ArgumentsFormat);
            if (processResult.ExitCode != 0)
            {
                var innerException = new Exception(processResult.CombinedOutput);
                throw ex;
            }

            return new CommandResult(processResult.ExitCode, processResult.CombinedOutput);
        }
    }
}
