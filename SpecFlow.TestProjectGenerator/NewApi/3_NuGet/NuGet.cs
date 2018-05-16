using System;
using System.IO;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._3_NuGet
{
    public class NuGet
    {
        private readonly Folders _folders;
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
            var processPath = Path.Combine(_folders.GlobalPackages, "NuGet.CommandLine", "4.5.1", "tools", "NuGet.exe");

            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException("NuGet.exe could not be found! Is the version number correct?", processPath);
            }

            var commandLineArgs = $"restore {_testProjectFolders.SolutionFileName} -SolutionDirectory . ";


            var nugetRestore = new ProcessHelper();
            var processResult = nugetRestore.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, processPath, commandLineArgs);

            if (processResult.ExitCode > 0)
            {
                throw new Exception("NuGet restore failed - rebuild solution to generate latest packages " + Environment.NewLine +
                                    $"{_testProjectFolders.PathToSolutionDirectory} {processPath} {commandLineArgs}" + Environment.NewLine + processResult.CombinedOutput);
            }
        }
    }
}
