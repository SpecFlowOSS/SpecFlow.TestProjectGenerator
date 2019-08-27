using System;
using System.IO;
using System.Runtime.InteropServices;

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
            ProcessResult processResult;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                 processResult = nugetRestore.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, processPath, commandLineArgs);
            }
            else
            {
                processResult = nugetRestore.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "/usr/bin/mono", processPath + " " + commandLineArgs);
            }


            if (processResult.ExitCode > 0)
            {
                throw new Exception("NuGet restore failed - rebuild solution to generate latest packages " + Environment.NewLine +
                                    $"{_testProjectFolders.PathToSolutionDirectory} {processPath} {commandLineArgs}" + Environment.NewLine + processResult.CombinedOutput);
            }
        }

        protected virtual string GetPathToNuGetExe()
        {
            return Path.Combine(_folders.GlobalPackages, "NuGet.CommandLine".ToLower(), "5.1.0", "tools", "NuGet.exe");
        }
    }
}
