using System;
using System.Collections.Generic;
using System.IO;
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

            var newProjCommand = DotNet.New()
                                       .Project()
                                       .InFolder(path)
                                       .WithName(project.Name)
                                       .WithLanguage(project.ProgrammingLanguage)
                                       .Build();

            var newProjResult = newProjCommand.Execute();

            if (newProjResult.ExitCode != 0)
            {
                throw new ProjectCreationNotPossibleException();
            }

            string projFileName;
            switch (project.ProgrammingLanguage)
            {
                case ProgrammingLanguage.CSharp:
                    projFileName = $"{project.Name}.csproj";
                    break;

                case ProgrammingLanguage.FSharp:
                    projFileName = $"{project.Name}.fsproj";
                    break;

                case ProgrammingLanguage.VB:
                    projFileName = $"{project.Name}.vbproj";
                    break;

                default: throw new ProjectCreationNotPossibleException();
            }

            string projFilePath = Path.Combine(path, projFileName);

            foreach (var nugetPackage in project.NuGetPackages)
            {
                var addPackageCommand = DotNet.Add()
                                              .Reference()
                                              .WithPackageName(nugetPackage.Name)
                                              .WithPackageVersion(nugetPackage.Version)
                                              .ToProject(projFilePath)
                                              .Build();
                if (addPackageCommand.Execute().ExitCode != 0)
                {
                    throw new ProjectCreationNotPossibleException();
                }
            }

            var fileWriter = new ProjectFileWriter();

            foreach (var file in project.Files)
            {
                fileWriter.Write(file, path);
            }
        }
    }
}
