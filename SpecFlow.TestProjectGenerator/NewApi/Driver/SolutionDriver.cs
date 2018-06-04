using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._2_Filesystem;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._3_NuGet;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._4_Compile;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class SolutionDriver
    {
        public const string DefaultProjectName = "DefaultTestProject";

        private readonly NuGetConfigGenerator _nuGetConfigGenerator;
        private readonly TestRunConfiguration _testRunConfiguration;
        private readonly ProjectBuilderFactory _projectBuilderFactory;
        private readonly Folders _folders;
        private readonly NuGet _nuGet;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly Compiler _compiler;
        private readonly IOutputWriter _outputWriter;
        private readonly Solution _solution;
        private readonly Dictionary<string, ProjectBuilder> _projects = new Dictionary<string, ProjectBuilder>();
        private bool _isWrittenOnDisk;
        private CompileResult _compileResult;

        public SolutionDriver(NuGetConfigGenerator nuGetConfigGenerator, TestRunConfiguration testRunConfiguration, ProjectBuilderFactory projectBuilderFactory, Folders folders, NuGet nuGet, TestProjectFolders testProjectFolders, Compiler compiler, IOutputWriter outputWriter)
        {
            _nuGetConfigGenerator = nuGetConfigGenerator;
            _testRunConfiguration = testRunConfiguration;
            _projectBuilderFactory = projectBuilderFactory;
            _folders = folders;
            _nuGet = nuGet;
            _testProjectFolders = testProjectFolders;
            _compiler = compiler;
            _outputWriter = outputWriter;
            NuGetSources = new List<NuGetSource> { new NuGetSource("LocalSpecFlowDevPackages", _folders.NuGetFolder) /*, new NuGetSource("LocalExternalPackages", _folders.ExternalNuGetFolder)*/ };
            _solution = new Solution(SolutionName);
            testProjectFolders.PathToSolutionFile = Path.Combine(_folders.FolderToSaveGeneratedSolutions, SolutionName, $"{SolutionName}.sln");
        }

        public Guid SolutionGuid { get; } = Guid.NewGuid();

        public IList<NuGetSource> NuGetSources { get; }

        public string SolutionName => $"TestSolution_{SolutionGuid:N}";

        public IReadOnlyDictionary<string, ProjectBuilder> Projects => _projects;

        private ProjectBuilder _defaultProject;
        public ProjectBuilder DefaultProject
        {
            get
            {
                if (_defaultProject == null)
                {
                    _defaultProject = _projectBuilderFactory.CreateProject(DefaultProjectName, _testRunConfiguration.ProgrammingLanguage);
                    _projects.Add(_defaultProject.ProjectName, _defaultProject);
                }

                return _defaultProject;
            }
        }

        public void AddProject(ProjectBuilder project)
        {
            if (_defaultProject == null)
            {
                _defaultProject = project;
            }

            _projects.Add(project.ProjectName, project);
        }

        public void CompileSolution()
        {
            foreach (var project in Projects.Values)
            {
                project.GenerateConfigurationFile();
            }

            WriteToDisk();
            _nuGet.Restore();

            _compileResult = _compiler.Run();
        }

        public void WriteToDisk()
        {
            if (_isWrittenOnDisk) return;

            foreach (var project in Projects.Values)
            {
                project.Build();
            }

            foreach (var project in Projects.Values)
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
