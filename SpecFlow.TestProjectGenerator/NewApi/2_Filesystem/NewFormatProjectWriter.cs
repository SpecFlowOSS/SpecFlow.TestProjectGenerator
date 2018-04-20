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
        public override string WriteProject(Project project, string projRootPath)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }
            
            CreateProjectFile(project, projRootPath);

            string projFileName = $"{project.Name}.{project.ProgrammingLanguage.ToProjectFileExtension()}";

            string projFilePath = Path.Combine(projRootPath, projFileName);
            
            WriteNuGetPackages(project, projFilePath);
            WriteProjectReferences(project, projFilePath);

            var doc = new XmlDocument();
            doc.Load(projFilePath);
            var projRootNode = doc.SelectSingleNode("//Project") ?? throw new ProjectCreationNotPossibleException("Project root node could not be found in project file.");
            
            WriteReferences(project, doc, projRootNode);
            SetTargetFramework(project, doc, projRootNode);

            doc.Save(projFilePath);
            
            WriteProjectFiles(project, projRootPath);

            return projFilePath;
        }

        private void WriteNuGetPackages(Project project, string projFilePath)
        {
            foreach (var nugetPackage in project.NuGetPackages)
            {
                var addPackageCommand = DotNet.Add()
                                              .Package()
                                              .WithPackageName(nugetPackage.Name)
                                              .WithPackageVersion(nugetPackage.Version)
                                              .ToProject(projFilePath)
                                              .Build();

                addPackageCommand.Execute(new ProjectCreationNotPossibleException($"Adding nuGet Package '{nugetPackage.Name}', {nugetPackage.Version} failed."));
            }
        }

        private void WriteProjectReferences(Project project, string projFilePath)
        {
            foreach (var projReference in project.ProjectReferences)
            {
                DotNet.Add()
                      .Reference()
                      .ReferencingProject(projReference.Path)
                      .ToProject(projFilePath)
                      .Build()
                      .Execute(new ProjectCreationNotPossibleException($"Adding refence to '{projReference.Path}' failed."));
            }
        }

        private void WriteProjectFiles(Project project, string projRootPath)
        {
            var fileWriter = new ProjectFileWriter();
            foreach (var file in project.Files)
            {
                fileWriter.Write(file, projRootPath);
            }
        }

        private void SetTargetFramework(Project project, XmlDocument doc, XmlNode projRootNode)
        {
            var targetFrameworkNode =
                projRootNode.SelectSingleNode("//PropertyGroup/TargetFramework")
                ?? throw new ProjectCreationNotPossibleException();

            string newTargetFrameworks = project.TargetFrameworks.ToTargetFrameworkMoniker();
            var targetFrameworkParentGroup =
                targetFrameworkNode.ParentNode ?? throw new ProjectCreationNotPossibleException();
            var multipleTargetFrameworksNode = doc.CreateElement("TargetFrameworks");
            multipleTargetFrameworksNode.InnerText = newTargetFrameworks;

            targetFrameworkParentGroup.RemoveChild(targetFrameworkNode);
            targetFrameworkParentGroup.AppendChild(multipleTargetFrameworksNode);
        }

        private void WriteReferences(Project project, XmlDocument doc, XmlNode projRootNode)
        {
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

                projRootNode.AppendChild(referencesNode);
            }
        }

        private void CreateProjectFile(Project project, string projRootPath)
        {
            string template = project.ProjectType == ProjectType.Exe ? "console" : "classlib";

            var newProjCommand = DotNet.New()
                                       .Project()
                                       .InFolder(projRootPath)
                                       .WithName(project.Name)
                                       .UsingTemplate(template)
                                       .WithLanguage(project.ProgrammingLanguage)
                                       .Build();
            
            newProjCommand.Execute(new ProjectCreationNotPossibleException("Execution of dotnet new failed."));
        }
    }
}
