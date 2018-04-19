using System;
using System.IO;
using System.Linq;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class SolutionWriter
    {
        public string WriteToFileSystem(Solution solution, string outputPath)
        {
            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            var createSolutionCommand = DotNet.New().Solution().InFolder(outputPath).WithName(solution.Name).Build();

            ExecuteCommandBuilder(
                createSolutionCommand,
                new ProjectCreationNotPossibleException("Could not create solution."));

            string solutionFilePath = Path.Combine(outputPath, $"{solution.Name}.sln");

            BaseProjectWriter newFormatProjectWriter = new NewFormatProjectWriter();
            BaseProjectWriter oldFormatProjectWriter = new OldFormatProjectWriter();

            foreach (var project in solution.Projects)
            {
                string projPath;
                switch (project.ProjectFormat)
                {
                    case ProjectFormat.New:
                        projPath = newFormatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));
                        break;
                    case ProjectFormat.Old:
                        projPath = oldFormatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));
                        break;

                    default: throw new ProjectCreationNotPossibleException("Unknown project format.");
                }

                var addProjCommand = DotNet.Sln().AddProject().Project(projPath).ToSolution(solutionFilePath).Build();
                if (addProjCommand.Execute().ExitCode != 0)
                {
                    throw new ProjectCreationNotPossibleException("Could not add project to solution.");
                }
            }

            //see ProjectCompiler.Compile

            return solutionFilePath; //path to solution file
        }

        public CommandResult ExecuteCommandBuilder(CommandBuilder cb, Exception ex)
        {
            var result = cb.Execute();

            if (result.ExitCode != 0)
            {
                throw ex;
            }

            return result;
        }

        public void WriteProjects(Solution solution, string outputPath, string solutionFilePath)
        {
            BaseProjectWriter newFormatProjectWriter = new NewFormatProjectWriter();
            BaseProjectWriter oldFormatProjectWriter = new OldFormatProjectWriter();

            foreach (var project in solution.Projects)
            {
                string projPath;
                switch (project.ProjectFormat)
                {
                    case ProjectFormat.New:
                        projPath = newFormatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));
                        break;
                    case ProjectFormat.Old:
                        projPath = oldFormatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));
                        break;

                    default: throw new ProjectCreationNotPossibleException("Unknown project format.");
                }

                var addProjCommand = DotNet.Sln().AddProject().Project(projPath).ToSolution(solutionFilePath).Build();
                if (addProjCommand.Execute().ExitCode != 0)
                {
                    throw new ProjectCreationNotPossibleException("Could not add project to solution.");
                }
            }
        }

        public void WriteProject(Project project, string outputPath, )
        {
            string projPath;
            switch (project.ProjectFormat)
            {
                case ProjectFormat.New:
                    projPath = newFormatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));
                    break;
                case ProjectFormat.Old:
                    projPath = oldFormatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));
                    break;

                default: throw new ProjectCreationNotPossibleException("Unknown project format.");
            }

            var addProjCommand = DotNet.Sln().AddProject().Project(projPath).ToSolution(solutionFilePath).Build();
            if (addProjCommand.Execute().ExitCode != 0)
            {
                throw new ProjectCreationNotPossibleException("Could not add project to solution.");
            }
        }
    }
}
