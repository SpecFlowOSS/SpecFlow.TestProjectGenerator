using System;
using System.IO;
using System.Linq;
using System.Xml;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class NewFormatProjectWriter : IProjectWriter
    {
        private readonly IOutputWriter _outputWriter;

        public NewFormatProjectWriter(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
        }

        public virtual string WriteProject(Project project, string projRootPath)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            CreateProjectFile(project, projRootPath);

            string projFileName = $"{project.Name}.{project.ProgrammingLanguage.ToProjectFileExtension()}";

            string projFilePath = Path.Combine(projRootPath, projFileName);

            var doc = new XmlDocument();
            doc.Load(projFilePath);
            var projRootNode = doc.SelectSingleNode("//Project") ?? throw new ProjectCreationNotPossibleException("Project root node could not be found in project file.");

            WriteReferences(project, doc, projRootNode);
            SetTargetFramework(project, doc, projRootNode);

            WriteFileReferences(project, projRootNode, doc);

            doc.Save(projFilePath);

            WriteNuGetPackages(project, projFilePath);

            WriteProjectFiles(project, projRootPath);

            return projFilePath;
        }

        private void WriteFileReferences(Project project, XmlNode projectNode, XmlDocument projectFileXmlDoc)
        {
            var itemGroup = projectFileXmlDoc.CreateElement("ItemGroup");

            if (project.ProgrammingLanguage == ProgrammingLanguage.FSharp)
            {
                foreach (var file in project.Files.Where(f => f.BuildAction.ToUpper() == "COMPILE"))
                {
                    var fileElement = projectFileXmlDoc.CreateElement("Compile");
                    fileElement.SetAttribute("Include", file.Path);

                    itemGroup.AppendChild(fileElement);
                }
            }
            
            foreach (var file in project.Files.Where(f => f.BuildAction.ToUpper() == "CONTENT" || f.BuildAction.ToUpper() == "NONE" && f.CopyToOutputDirectory != CopyToOutputDirectory.DoNotCopy))
            {
                var fileElement = projectFileXmlDoc.CreateElement(file.BuildAction);
                fileElement.SetAttribute("Include", file.Path);

                var copyToOutputDirElement = projectFileXmlDoc.CreateElement("CopyToOutputDirectory");
                copyToOutputDirElement.InnerText = file.CopyToOutputDirectory.GetCopyToOutputDirectoryString();

                fileElement.AppendChild(copyToOutputDirElement);
                itemGroup.AppendChild(fileElement);
            }

            projectNode.AppendChild(itemGroup);
        }

        public void WriteReferences(Project project, string projectFilePath)
        {
            WriteProjectReferences(project, projectFilePath);
        }

        private void WriteNuGetPackages(Project project, string projFilePath)
        {
            foreach (var nugetPackage in project.NuGetPackages)
            {
                var addPackageCommand = DotNet.Add(_outputWriter)
                                              .Package()
                                              .WithPackageName(nugetPackage.Name)
                                              .WithPackageVersion(nugetPackage.Version)
                                              .ToProject(projFilePath)
                                              .WithNoRestore()
                                              .Build();

                addPackageCommand.Execute(new ProjectCreationNotPossibleException($"Adding nuGet Package '{nugetPackage.Name}', {nugetPackage.Version} failed."));
            }
        }

        private void WriteProjectReferences(Project project, string projFilePath)
        {
            if (project.ProjectReferences.Count > 0)
            {
                var reference = DotNet.Add(_outputWriter)
                                      .Reference();
                foreach (var projReference in project.ProjectReferences)
                {
                    reference.ReferencingProject(projReference.Path);
                }

                reference.ToProject(projFilePath)
                         .Build()
                         .Execute(new ProjectCreationNotPossibleException($"Writing ProjectRefences failed."));
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
            var targetFrameworkNode = projRootNode.SelectSingleNode("//PropertyGroup/TargetFramework") ?? throw new ProjectCreationNotPossibleException();
            string newTargetFrameworks = project.TargetFrameworks.ToTargetFrameworkMoniker();

            targetFrameworkNode.InnerText = newTargetFrameworks;

            //var targetFrameworkParentGroup = targetFrameworkNode.ParentNode ?? throw new ProjectCreationNotPossibleException();
            //var multipleTargetFrameworksNode = doc.CreateElement("TargetFrameworks");
            //multipleTargetFrameworksNode.InnerText = newTargetFrameworks;

            //targetFrameworkParentGroup.RemoveChild(targetFrameworkNode);
            //targetFrameworkParentGroup.AppendChild(multipleTargetFrameworksNode);
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

            var newProjCommand = DotNet.New(_outputWriter)
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
