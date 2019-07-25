using System;
using System.IO;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class NuGet
    {
        protected readonly Folders _folders;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly IOutputWriter _outputWriter;

        public NuGet(Folders folders, TestProjectFolders testProjectFolders, IOutputWriter outputWriter)
        {
            _folders = folders;
            _testProjectFolders = testProjectFolders;
            _outputWriter = outputWriter;
        }

        public void Restore()
        {
            var processPath = GetPathToNuGetExe();

            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException("NuGet.exe could not be found! Is the version number correct?", processPath);
            }

            var commandLineArgs = $"restore {_testProjectFolders.SolutionFileName} -SolutionDirectory . -NoCache";


            var nugetRestore = new ProcessHelper();
            var processResult = nugetRestore.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, processPath, commandLineArgs);

            if (processResult.ExitCode > 0)
            {
                throw new Exception("NuGet restore failed - rebuild solution to generate latest packages " + Environment.NewLine +
                                    $"{_testProjectFolders.PathToSolutionDirectory} {processPath} {commandLineArgs}" + Environment.NewLine + processResult.CombinedOutput);
            }
        }

        protected virtual string GetPathToNuGetExe()
        {
            return Path.Combine(_folders.GlobalPackages, "NuGet.CommandLine", "5.1.0", "tools", "NuGet.exe");
        }
    }
}
