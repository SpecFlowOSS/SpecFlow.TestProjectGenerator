using System;
using System.IO;
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

            string template = project.ProjectType == ProjectType.Exe ? "console" : "classlib";

            var newProjCommand = DotNet.New()
                                       .Project()
                                       .InFolder(path)
                                       .WithName(project.Name)
                                       .UsingTemplate(template)
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

            // nuget packages
            foreach (var nugetPackage in project.NuGetPackages)
            {
                var addPackageCommand = DotNet.Add()
                                              .Package()
                                              .WithPackageName(nugetPackage.Name)
                                              .WithPackageVersion(nugetPackage.Version)
                                              .ToProject(projFilePath)
                                              .Build();
                if (addPackageCommand.Execute().ExitCode != 0)
                {
                    throw new ProjectCreationNotPossibleException();
                }
            }

            // p2p references
            foreach (var projReference in project.ProjectReferences)
            {
                var addReferenceCommand = DotNet.Add()
                                                .Reference()
                                                .ReferencingProject(projReference.Path)
                                                .ToProject(projFilePath)
                                                .Build();

                if (addReferenceCommand.Execute().ExitCode != 0)
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
