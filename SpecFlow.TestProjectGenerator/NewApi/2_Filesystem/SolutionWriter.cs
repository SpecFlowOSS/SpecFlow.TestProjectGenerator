using System;
using System.IO;
using System.Linq;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem;
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

            //vb | csharp
            //new | old format

            

            //folder
            //files
            //  feature
            //  code //ProgrammLanguageDrivers
            //  app.config
            //  package.config
            // project 
            // nuget.config
            // solution //always dotnet sln

            var createSolutionCommand = DotNet.New().Solution().InFolder(outputPath).WithName(solution.Name).Build();

            var createSolutionResult = createSolutionCommand.Execute();

            if (createSolutionResult.ExitCode != 0)
            {
                throw new ProjectCreationNotPossibleException();
            }
            
            BaseProjectWriter newFormatProjectWriter = new NewFormatProjectWriter();
            BaseProjectWriter oldFormatProjectWriter = new OldFormatProjectWriter();

            foreach (var project in solution.Projects)
            {
                if (project.ProjectFormat == ProjectFormat.New)
                {
                    newFormatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));
                }
                else if (project.ProjectFormat == ProjectFormat.Old)
                {
                    oldFormatProjectWriter.WriteProject(project, Path.Combine(outputPath, project.Name));
                }
                else
                {
                    throw new ProjectCreationNotPossibleException();
                }
            }


            //see ProjectCompiler.Compile

            return Path.Combine(outputPath, $"{solution.Name}.sln"); //path to solution file
        }
    }
}
