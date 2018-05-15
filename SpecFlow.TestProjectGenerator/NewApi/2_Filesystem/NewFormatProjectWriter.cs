using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SpecFlow.TestProjectGenerator.Helpers;
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

            string projectFilePath = Path.Combine(projRootPath, projFileName);

            var xd = XDocument.Load(projectFilePath);
            var projectElement = xd.Element(XName.Get("Project")) ?? throw new ProjectCreationNotPossibleException($"No 'Project' tag could be found in project file '{projectFilePath}'");

            SetTargetFramework(project, projectElement);
            WriteAssemblyReferences(project, projectElement);
            WriteNuGetPackages(project, projectElement);
            WriteFileReferences(project, projectElement);

            xd.Save(projectFilePath);

            WriteProjectFiles(project, projRootPath);

            return projectFilePath;
        }

        private void WriteFileReferences(Project project, XElement projectElement)
        {
            var itemGroup = new XElement("ItemGroup");
            using (var xw = itemGroup.CreateWriter())
            {
                if (project.ProgrammingLanguage == ProgrammingLanguage.FSharp)
                {
                    foreach (var file in project.Files.Where(f => f.BuildAction.ToUpper() == "COMPILE"))
                    {
                        WriteFileReference(xw, file);
                    }
                }

                foreach (var file in project.Files.Where(f => f.BuildAction.ToUpper() == "CONTENT" || f.BuildAction.ToUpper() == "NONE" && f.CopyToOutputDirectory != CopyToOutputDirectory.DoNotCopy))
                {
                    WriteFileReference(xw, file);
                }
            }

            projectElement.Add(itemGroup);
        }

        private void WriteFileReference(XmlWriter xw, ProjectFile projectFile)
        {
            xw.WriteStartElement(projectFile.BuildAction);
            xw.WriteAttributeString("Include", projectFile.Path);

            if (projectFile.CopyToOutputDirectory != CopyToOutputDirectory.DoNotCopy)
            {
                xw.WriteElementString("CopyToOutputDirectory", projectFile.CopyToOutputDirectory.GetCopyToOutputDirectoryString());
            }
            
            xw.WriteEndElement();
        }

        public void WriteReferences(Project project, string projectFilePath)
        {
            WriteProjectReferences(project, projectFilePath);
        }

        private void WriteNuGetPackages(Project project, XElement projectElement)
        {
            var newNode = new XElement("ItemGroup");

            using (var xw = newNode.CreateWriter())
            {
                foreach (var nugetPackage in project.NuGetPackages)
                {
                    WritePackageReference(xw, nugetPackage);
                }
            }

            projectElement.Add(newNode);
        }

        private void WritePackageReference(XmlWriter xw, NuGetPackage nuGetPackage)
        {
            xw.WriteStartElement("PackageReference");
            xw.WriteAttributeString("Include", nuGetPackage.Name);

            if (nuGetPackage.Version.IsNotNullOrWhiteSpace())
            {
                xw.WriteAttributeString("Version", nuGetPackage.Version);
            }

            xw.WriteEndElement();
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

        private void SetTargetFramework(Project project, XElement projectElement)
        {
            var targetFrameworkElement = projectElement.Element("PropertyGroup")?.Element("TargetFramework") ?? throw new ProjectCreationNotPossibleException();

            string newTargetFrameworks = project.TargetFrameworks.ToTargetFrameworkMoniker();
            targetFrameworkElement.SetValue(newTargetFrameworks);

            //var targetFrameworkParentGroup = targetFrameworkNode.ParentNode ?? throw new ProjectCreationNotPossibleException();
            //var multipleTargetFrameworksNode = doc.CreateElement("TargetFrameworks");
            //multipleTargetFrameworksNode.InnerText = newTargetFrameworks;

            //targetFrameworkParentGroup.RemoveChild(targetFrameworkNode);
            //targetFrameworkParentGroup.AppendChild(multipleTargetFrameworksNode);
        }

        private void WriteAssemblyReferences(Project project, XElement projectElement)
        {
            // GAC and library references cannot be added in new Csproj format (via dotnet CLI)
            // see https://github.com/dotnet/sdk/issues/987
            // Therefore, write them manually into the project file
            var itemGroup = new XElement("ItemGroup");

            using (var xw = itemGroup.CreateWriter())
            {
                foreach (var reference in project.References)
                {
                    WriteAssemblyReference(xw, reference);
                }
            }

            projectElement.Add(itemGroup);
        }

        private void WriteAssemblyReference(XmlWriter xw, Reference reference)
        {
            xw.WriteStartElement("Reference");
            xw.WriteAttributeString("Include", reference.Name);
            xw.WriteEndElement();
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
