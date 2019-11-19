using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Factories;
using TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class SolutionDriver
    {
        public const string DefaultProjectName = "DefaultTestProject";

        private readonly NuGetConfigGenerator _nuGetConfigGenerator;
        private readonly TestRunConfiguration _testRunConfiguration;
        private readonly ProjectBuilderFactory _projectBuilderFactory;
        private readonly Folders _folders;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly Compiler _compiler;
        private readonly SolutionWriter _solutionWriter;
        private readonly NuGetRestorerFactory _nugetRestorerFactory;
        private readonly Solution _solution;
        private readonly Dictionary<string, ProjectBuilder> _projects = new Dictionary<string, ProjectBuilder>();
        private bool _isWrittenOnDisk;
        private CompileResult _compileResult;
        private ProjectBuilder _defaultProject;

        public SolutionDriver(
            NuGetConfigGenerator nuGetConfigGenerator,
            TestRunConfiguration testRunConfiguration,
            ProjectBuilderFactory projectBuilderFactory,
            Folders folders,
            TestProjectFolders testProjectFolders,
            Compiler compiler,
            SolutionWriter solutionWriter,
            NuGetRestorerFactory nugetRestorerFactory)
        {
            _nuGetConfigGenerator = nuGetConfigGenerator;
            _testRunConfiguration = testRunConfiguration;
            _projectBuilderFactory = projectBuilderFactory;
            _folders = folders;
            _testProjectFolders = testProjectFolders;
            _compiler = compiler;
            _solutionWriter = solutionWriter;
            _nugetRestorerFactory = nugetRestorerFactory;
            NuGetSources = new List<NuGetSource>
            {
                new NuGetSource("LocalSpecFlowDevPackages", _folders.NuGetFolder)
            };

            if (testRunConfiguration.UnitTestProvider == UnitTestProvider.SpecRun)
            {
                NuGetSources.Add(new NuGetSource("SpecFlow CI", "https://www.myget.org/F/specflow/api/v3/index.json"));
                NuGetSources.Add(new NuGetSource("SpecFlow Unstable", "https://www.myget.org/F/specflow-unstable/api/v3/index.json"));
            }

            _solution = new Solution(SolutionName);
            testProjectFolders.PathToSolutionFile = Path.Combine(_folders.FolderToSaveGeneratedSolutions, SolutionName, $"{SolutionName}.sln");
        }

        public Guid SolutionGuid { get; } = Guid.NewGuid();

        public IList<NuGetSource> NuGetSources { get; }

        public string SolutionName => $"S{SolutionGuid.ToString("N").Substring(24)}";

        public IReadOnlyDictionary<string, ProjectBuilder> Projects => _projects;

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

        public Solution GetSolution()
        {
            foreach (var project in Projects.Values)
            {
                project.Build();
            }

            foreach (var project in Projects.Values)
            {
                _solution.AddProject(project.Build());
            }

            _solution.NugetConfig = _nuGetConfigGenerator?.Generate(NuGetSources.ToArray());
            return _solution;
        }

        public void AddProject(ProjectBuilder project)
        {
            if (_defaultProject == null)
            {
                _defaultProject = project;
            }

            _projects.Add(project.ProjectName, project);
        }

        public void AddFile(string name, string content)
        {
            _solution.Files.Add(new SolutionFile(name, content));
        }

        public void CompileSolution(BuildTool buildTool, bool? treatWarningsAsErrors = null)
        {
            foreach (var project in Projects.Values)
            {
                project.IsTreatWarningsAsErrors = treatWarningsAsErrors;
                project.GenerateConfigurationFile();
            }

            WriteToDisk();

            _compileResult = _compiler.Run(buildTool, treatWarningsAsErrors);
        }

        public void WriteToDisk()
        {
            if (_isWrittenOnDisk)
            {
                return;
            }

            foreach (var project in Projects.Values)
            {
                project.Build();
            }

            foreach (var project in Projects.Values)
            {
                _solution.AddProject(project.Build());
            }

            _solution.NugetConfig = _nuGetConfigGenerator?.Generate(NuGetSources.ToArray());

            _solutionWriter.WriteToFileSystem(_solution, _testProjectFolders.PathToSolutionDirectory);

            foreach (var project in _solution.Projects)
            {
                var nugetRestorerForProject = _nugetRestorerFactory.GetNuGetRestorerForProject(project);
                nugetRestorerForProject.RestoreForProject(project);
            }

            _isWrittenOnDisk = true;
        }

        public void CheckSolutionShouldHaveCompiled()
        {
            _compileResult.Should().NotBeNull("the project should have compiled");
            _compileResult.IsSuccessful.Should().BeTrue("the project should have compiled successfully.\r\n\r\n------ Build output ------\r\n{0}", _compileResult.Output);
        }

        public void CheckSolutionShouldHaveCompileError()
        {
            _compileResult.Should().NotBeNull("the project should have compiled");
            _compileResult.IsSuccessful.Should().BeFalse("There should be a compile error");
        }

        public bool CheckCompileOutputForString(string str)
        {
            return _compileResult.Output.Contains(str);
        }
    }
}
