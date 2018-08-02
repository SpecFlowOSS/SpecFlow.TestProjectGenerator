using System;
using System.Collections.Generic;
using System.IO;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class SolutionWriter
    {
        private readonly IOutputWriter _outputWriter;
        private readonly ProjectWriterFactory _projectWriterFactory;
        private readonly ProjectFileWriter _projectFileWriter;

        public SolutionWriter(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
            _projectWriterFactory = new ProjectWriterFactory(outputWriter);
            _projectFileWriter = new ProjectFileWriter();
        }

        public string WriteToFileSystem(Solution solution, string outputPath)
        {
            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            var createSolutionCommand = DotNet.New(_outputWriter).Solution().InFolder(outputPath).WithName(solution.Name).Build();
            createSolutionCommand.Execute(new ProjectCreationNotPossibleException("Could not create solution."));
            string solutionFilePath = Path.Combine(outputPath, $"{solution.Name}.sln");

            WriteProjects(solution, outputPath, solutionFilePath);

            if (solution.NugetConfig != null)
            {
                _projectFileWriter.Write(solution.NugetConfig, outputPath);
            }

            //_projectFileWriter.Write(new ProjectFile("global.json", "None", "{ \"sdk\": { \"version\": \"2.1.105\" }}"), outputPath);

            return solutionFilePath;
        }

        private void WriteProjects(Solution solution, string outputPath, string solutionFilePath)
        {
            var projectPathMappings = new Dictionary<Project, string>();
            foreach (var project in solution.Projects)
            {
                var formatProjectWriter = _projectWriterFactory.FromProjectFormat(project.ProjectFormat);
                var pathToProjectFile = WriteProject(project, outputPath, formatProjectWriter, solutionFilePath);
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
