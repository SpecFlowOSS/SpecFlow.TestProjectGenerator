using System;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class Compiler
    {
        private readonly VisualStudioFinder _visualStudioFinder;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly IOutputWriter _outputWriter;

        public Compiler(VisualStudioFinder visualStudioFinder, TestProjectFolders testProjectFolders, IOutputWriter outputWriter)
        {
            _visualStudioFinder = visualStudioFinder;
            _testProjectFolders = testProjectFolders;
            _outputWriter = outputWriter;
        }

        public CompileResult Run(BuildTool buildTool)
        {
            switch (buildTool)
            {
                case BuildTool.MSBuild:
                    return CompileWithMSBuild();
                case BuildTool.DotnetBuild:
                    return CompileWithDotnetBuild();
                case BuildTool.DotnetMSBuild:
                    return CompileWithDotnetMSBuild();
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildTool), buildTool, null);
            }
        }

        private CompileResult CompileWithMSBuild()
        {
            string msBuildPath="";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                msBuildPath = _visualStudioFinder.FindMSBuild();
            }
            else
            {
                msBuildPath = "sudo";
            }

            _outputWriter.WriteLine($"Invoke MsBuild from {msBuildPath}");

            var processHelper = new ProcessHelper();
            var msBuildProcess = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, msBuildPath, 
                $"{(Environment.OSVersion.Platform == PlatformID.Unix ? "/usr/share/dotnet/dotnet msbuild" : "")} -restore -bl -nologo -v:m \"{_testProjectFolders.PathToSolutionFile}\"");

            return new CompileResult(msBuildProcess.ExitCode, msBuildProcess.CombinedOutput);
        }

        private CompileResult CompileWithDotnetBuild()
        {
            _outputWriter.WriteLine($"Invoke dotnet build ");

            var processHelper = new ProcessHelper();
            var msBuildProcess = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "dotnet", $"build \"{_testProjectFolders.PathToSolutionFile}\"");

            return new CompileResult(msBuildProcess.ExitCode, msBuildProcess.CombinedOutput);
        }

        private CompileResult CompileWithDotnetMSBuild()
        {
            _outputWriter.WriteLine($"Invoke dotnet msbuild ");

            var processHelper = new ProcessHelper();
            var msBuildProcess = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "dotnet", $"msbuild /bl \"{_testProjectFolders.PathToSolutionFile}\"");

            return new CompileResult(msBuildProcess.ExitCode, msBuildProcess.CombinedOutput);
        }

    }
}
