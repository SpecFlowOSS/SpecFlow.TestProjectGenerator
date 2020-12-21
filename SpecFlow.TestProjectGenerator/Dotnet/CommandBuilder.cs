using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.Dotnet
{
    public class CommandBuilder
    {
        private readonly IOutputWriter _outputWriter;
        private readonly string _workingDirectory;

        public CommandBuilder(IOutputWriter outputWriter, string executablePath, string argumentsFormat, string workingDirectory)
        {
            _outputWriter = outputWriter;
            _workingDirectory = workingDirectory;
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

        public CommandResult Execute(Func<Exception, Exception> exceptionFunction) 
        {
            var solutionCreateProcessHelper = new ProcessHelper();

            var processResult = solutionCreateProcessHelper.RunProcess(_outputWriter, _workingDirectory, ExecutablePath, ArgumentsFormat);
            if (processResult.ExitCode != 0)
            {
                var innerException = new Exception(processResult.CombinedOutput);

                throw exceptionFunction(innerException);
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
