using System;
using System.IO;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class SolutionWriter
    {
        private readonly ProjectWriterFactory _projectWriterFactory = new ProjectWriterFactory();
        private readonly ProjectFileWriter _projectFileWriter;

        public SolutionWriter()
        {
            _projectFileWriter = new ProjectFileWriter();
        }

        public string WriteToFileSystem(Solution solution, string outputPath)
        {
            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            var createSolutionCommand = DotNet.New().Solution().InFolder(outputPath).WithName(solution.Name).Build();

            createSolutionCommand.Execute(new ProjectCreationNotPossibleException("Could not create solution."));

            string solutionFilePath = Path.Combine(outputPath, $"{solution.Name}.sln");

            WriteProjects(solution, outputPath, solutionFilePath);

            if (solution.NugetConfig != null)
            {
                _projectFileWriter.Write(solution.NugetConfig, outputPath);
            }

            return solutionFilePath;
        }

        private void WriteProjects(Solution solution, string outputPath, string solutionFilePath)
        {
            foreach (var project in solution.Projects)
            {
                var formatProjectWriter = _projectWriterFactory.FromProjectFormat(project.ProjectFormat);
                WriteProject(project, outputPath, formatProjectWriter, solutionFilePath);
            }
        }

        private void WriteProject(Project project, string outputPath, BaseProjectWriter formatProjectWriter, string solutionFilePath)
        {
            string projPath = formatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));

            var addProjCommand = DotNet.Sln().AddProject().Project(projPath).ToSolution(solutionFilePath).Build().Execute();
            if (addProjCommand.ExitCode != 0)
            {
                throw new ProjectCreationNotPossibleException("Could not add project to solution.");
            }
        }
    }
}
