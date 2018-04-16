using System;
using System.IO;
using System.Xml;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class NewFormatProjectWriter : BaseProjectWriter
    {
        public override string WriteProject(Project project, string path)
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

            string projFileName = $"{project.Name}.{project.ProgrammingLanguage.ToProjectFileExtension()}";

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

            var doc = new XmlDocument();
            doc.Load(projFilePath);
            var projRoot = doc.SelectSingleNode("//Project") ?? throw new ProjectCreationNotPossibleException();

            // GAC and library references cannot be added in new Csproj format (via dotnet CLI)
            // see https://github.com/dotnet/sdk/issues/987
            // Therefore, write them manually into the project file
            if (project.References.Count > 0)
            {
                var referencesNode = doc.CreateElement("ItemGroup");

                foreach (var reference in project.References)
                {
                    var node = doc.CreateElement("Reference");
                    node.SetAttribute("Include", reference.Name);
                    referencesNode.AppendChild(node);
                }

                projRoot.AppendChild(referencesNode);
            }

            // set target framework moniker
            var targetFrameworkNode =
                projRoot.SelectSingleNode("//PropertyGroup/TargetFramework") ?? throw new ProjectCreationNotPossibleException();

            string newTargetFrameworks = project.TargetFrameworks.ToTargetFrameworkMoniker();
            var targetFrameworkParentGroup =
                targetFrameworkNode.ParentNode ?? throw new ProjectCreationNotPossibleException();
            var multipleTargetFrameworksNode = doc.CreateElement("TargetFrameworks");
            multipleTargetFrameworksNode.InnerText = newTargetFrameworks;

            targetFrameworkParentGroup.RemoveChild(targetFrameworkNode);
            targetFrameworkParentGroup.AppendChild(multipleTargetFrameworksNode);
            
            doc.Save(projFilePath);

            // write project files
            var fileWriter = new ProjectFileWriter();
            foreach (var file in project.Files)
            {
                fileWriter.Write(file, path);
            }

            return projFilePath;
        }
    }
}
