using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class NewFormatProjectWriter : BaseProjectWriter
    {
        public override void WriteProject(Project project, string path)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var commandBuilder = DotNet.New().Project().InFolder(path).WithName(project.Name).UseTemplate("classlib").WithLanguage(project.ProgrammingLanguage).Build();
            var commandResult = commandBuilder.Execute();

            if (commandResult.ExitCode != 0)
            {
                throw new ProjectCreationNotPossibleException();
            }

            var fileWriter = new ProjectFileWriter();

            foreach (var file in project.Files)
            {
                fileWriter.Write(file, path);
            }
        }
    }
}
