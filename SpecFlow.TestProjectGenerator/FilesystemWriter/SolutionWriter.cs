using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Dotnet;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class SolutionWriter
    {
        private readonly IOutputWriter _outputWriter;
        private readonly ProjectWriterFactory _projectWriterFactory;
        private readonly ProjectFileWriter _projectFileWriter;
        private readonly TargetFrameworkMonikerStringBuilder _targetFrameworkMonikerStringBuilder;
        private readonly NetCoreSdkInfoProvider _netCoreSdkInfoProvider;
        private readonly TargetFrameworkSplitter _targetFrameworkSplitter;
        private readonly TargetFrameworkVersionStringBuilder _targetFrameworkVersionStringBuilder;

        public SolutionWriter(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
            _targetFrameworkSplitter = new TargetFrameworkSplitter();
            _targetFrameworkMonikerStringBuilder = new TargetFrameworkMonikerStringBuilder(_targetFrameworkSplitter);
            _targetFrameworkVersionStringBuilder = new TargetFrameworkVersionStringBuilder(_targetFrameworkSplitter);
            _projectWriterFactory = new ProjectWriterFactory(outputWriter, _targetFrameworkMonikerStringBuilder, _targetFrameworkVersionStringBuilder);
            _projectFileWriter = new ProjectFileWriter();
            _netCoreSdkInfoProvider = new NetCoreSdkInfoProvider();
        }

        public string WriteToFileSystem(Solution solution, string outputPath)
        {
            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            var createSolutionCommand = DotNet.New(_outputWriter).Solution().InFolder(outputPath).WithName(solution.Name).Build();
            createSolutionCommand.Execute((innerException) => new ProjectCreationNotPossibleException("Could not create solution.", innerException));
            string solutionFilePath = Path.Combine(outputPath, $"{solution.Name}.sln");

            WriteProjects(solution, outputPath, solutionFilePath);

            if (solution.NugetConfig != null)
            {
                _projectFileWriter.Write(solution.NugetConfig, outputPath);
            }

            string maxTargetFrameworkMoniker =
                solution.Projects
                        .Select(p => p.TargetFrameworks)
                        .SelectMany(_targetFrameworkMonikerStringBuilder.GetAllTargetFrameworkMonikers)
                        .FirstOrDefault();

            if (maxTargetFrameworkMoniker is string tfm)
            {
                var sdk = _netCoreSdkInfoProvider.GetSdkFromTargetFramework(tfm);
                var globalJsonBuilder = new GlobalJsonBuilder().WithSdk(sdk);

                var globalJsonFile = globalJsonBuilder.ToProjectFile();
                _projectFileWriter.Write(globalJsonFile, outputPath);
            }

            return solutionFilePath;
        }

        private void WriteProjects(Solution solution, string outputPath, string solutionFilePath)
        {
            var projectPathMappings = new Dictionary<Project, string>();
            foreach (var project in solution.Projects)
            {
                var formatProjectWriter = _projectWriterFactory.FromProjectFormat(project.ProjectFormat);
                string pathToProjectFile = WriteProject(project, outputPath, formatProjectWriter, solutionFilePath);
                projectPathMappings.Add(project, pathToProjectFile);
            }

            foreach (var project in solution.Projects)
            {
                var formatProjectWriter = _projectWriterFactory.FromProjectFormat(project.ProjectFormat);
                formatProjectWriter.WriteReferences(project, projectPathMappings[project]);
            }
        }

        private string WriteProject(Project project, string outputPath, IProjectWriter formatProjectWriter, string solutionFilePath)
        {
            string projPath = formatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));

            var addProjCommand = DotNet.Sln(_outputWriter).AddProject().Project(projPath).ToSolution(solutionFilePath).Build().Execute();
            if (addProjCommand.ExitCode != 0)
            {
                throw new ProjectCreationNotPossibleException("Could not add project to solution.");
            }

            return projPath;
        }
    }
}
