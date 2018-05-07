using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem;
using SpecFlow.TestProjectGenerator.NewApi._3_NuGet;
using SpecFlow.TestProjectGenerator.NewApi._4_Compile;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class SolutionDriver
    {
        private readonly NuGetConfigGenerator _nuGetConfigGenerator;
        private readonly Folders _folders;
        private readonly NuGet _nuGet;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly ProjectsDriver _projectsDriver;
        private readonly Compiler _compiler;
        private readonly IOutputWriter _outputWriter;
        private readonly Solution _solution;
        private bool _isWrittenOnDisk;
        private CompileResult _compileResult;

        public SolutionDriver(NuGetConfigGenerator nuGetConfigGenerator, Folders folders, NuGet nuGet, TestProjectFolders testProjectFolders, ProjectsDriver projectsDriver, Compiler compiler, IOutputWriter outputWriter)
        {
            _nuGetConfigGenerator = nuGetConfigGenerator;
            _folders = folders;
            _nuGet = nuGet;
            _testProjectFolders = testProjectFolders;
            _projectsDriver = projectsDriver;
            _compiler = compiler;
            _outputWriter = outputWriter;
            NuGetSources = new List<NuGetSource> { new NuGetSource("LocalSpecFlowDevPackages", _folders.NuGetFolder), new NuGetSource("LocalExternalPackages", _folders.ExternalNuGetFolder) };
            _solution = new Solution(SolutionName);
            testProjectFolders.PathToSolutionFile = Path.Combine(_folders.FolderToSaveGeneratedSolutions, SolutionName, $"{SolutionName}.sln");
        }

        public Guid SolutionGuid { get; } = Guid.NewGuid();

        public IList<NuGetSource> NuGetSources { get; }

        public string SolutionName => $"TestSolution_{SolutionGuid:N}";

        public void CompileSolution()
        {
            foreach (var project in _projectsDriver.Projects.Values)
            {
                project.GenerateConfigurationFile();
            }

            WriteToDisk();
            _nuGet.Restore();

            _compileResult = _compiler.Run();
            if (!_compileResult.IsSuccessful)
            {
                throw new InvalidOperationException($"Failed compiling. {_compileResult.Output}");
            }
        }

        public void WriteToDisk()
        {
            if (_isWrittenOnDisk) return;

            foreach (var project in _projectsDriver.Projects.Values)
            {
                _solution.AddProject(project.Build());
            }

            _solution.NugetConfig = _nuGetConfigGenerator?.Generate(NuGetSources.ToArray());

            var solutionWriter = new SolutionWriter(_outputWriter);
            solutionWriter.WriteToFileSystem(_solution, _testProjectFolders.PathToSolutionDirectory);
            // TODO: search all projects for unit tests
            var proj = _solution.Projects.First();

            _testProjectFolders.ProjectFolder = Path.Combine(_testProjectFolders.PathToSolutionDirectory, proj.Name);
            _testProjectFolders.ProjectBinOutputPath = Path.Combine(_testProjectFolders.ProjectFolder, GetProjectCompilePath(proj));
            _testProjectFolders.TestAssemblyFileName = $"{proj.Name}.dll";
            _testProjectFolders.PathToNuGetPackages = proj.ProjectFormat == ProjectFormat.Old
                ? Path.Combine(_testProjectFolders.PathToSolutionDirectory, "packages")
                : _folders.GlobalPackages;

            _isWrittenOnDisk = true;
        }

        public void CheckSolutionShouldHaveCompiled()
        {
            _compileResult.Should().NotBeNull("the project should have compiled");
            _compileResult.IsSuccessful.Should().BeTrue("the project should have compiled successfully.\r\n\r\n------ Build output ------\r\n{0}", _compileResult.Output);
        }

        private string GetProjectCompilePath(Project project)
        {
            // TODO: hardcoded "Debug" value should be replaced by a configuration parameter
            if (project.ProjectFormat == ProjectFormat.New)
            {
                return Path.Combine("bin", "Debug", project.TargetFrameworks.ToTargetFrameworkMoniker().Split(';')[0]);
            }

            return Path.Combine("bin", "Debug");
        }
    }
}
