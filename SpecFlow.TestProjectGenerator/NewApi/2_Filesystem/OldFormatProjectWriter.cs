using System;
using System.IO;
using System.Text;
using System.Xml;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class OldFormatProjectWriter : BaseProjectWriter
    {
        public override string WriteProject(Project project, string path)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            Directory.CreateDirectory(path);
            string projFileName = $"{project.Name}.{project.ProgrammingLanguage.ToProjectFileExtension()}";
            string projFilePath = Path.Combine(path, projFileName);

            const string msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

            using (var xw = new XmlTextWriter(
                File.Open(projFilePath, FileMode.Create, FileAccess.Write, FileShare.Read),
                Encoding.UTF8))
            {
                xw.WriteStartDocument();

                // project tag
                xw.WriteStartElement(string.Empty, "Project", msbuildNamespace);
                xw.WriteAttributeString("ToolsVersion", "14.0");
                xw.WriteAttributeString("DefaultTargets", "Build");

                // write main project properties
                WriteProjectProperties(xw, project);

                // write GAC & assembly references
                WriteProjectReferences(xw, project);

                // write NuGet package references
                WriteProjectNuGetPackages(xw, project);

                // write imports from NuGet packages
                WriteProjectNuGetPackageImports(xw, project);

                // write project files
                WriteProjectFiles(xw, project, path);

                // close project tag
                xw.WriteEndElement();
                xw.WriteEndDocument();
            }

            return projFilePath;
        }

        private void WriteProjectProperties(XmlWriter xw, Project project)
        {
            string targetFramework;
            try
            {
                targetFramework = project.TargetFrameworks.ToOldNetVersion();
            }
            catch (InvalidOperationException exc)
            {
                throw new ProjectCreationNotPossibleException("Multiple target frameworks don't work with the old csproj format", exc);
            }

            string outputType = project.ProjectType == ProjectType.Exe ? "WinExe" : "Library";

            // main property group
            xw.WriteStartElement("PropertyGroup");
            xw.WriteElementString("OutputType", outputType);
            xw.WriteElementString("RootNamespace", project.Name);
            xw.WriteElementString("AssemblyName", project.Name);
            xw.WriteElementString("FileAlignment", "512");
            xw.WriteElementString("TargetFrameworkVersion", targetFramework);

            // close main property group
            xw.WriteEndElement();
        }

        private void WriteProjectReferences(XmlWriter xw, Project project)
        {
            if (project.References.Count <= 0) return;

            // open item group for library & GAC references
            xw.WriteStartElement("ItemGroup");

            // write GAC references
            foreach (var reference in project.References)
            {
                xw.WriteStartElement("Reference");
                xw.WriteAttributeString("Include", reference.Name);
                xw.WriteEndElement();
            }

            // close item group for library & GAC references
            xw.WriteEndElement();
        }

        private void WriteProjectNuGetPackages(XmlWriter xw, Project project)
        {
            if (project.NuGetPackages.Count <= 0) return;

            // open item group for nuget packages
            xw.WriteStartElement("ItemGroup");

            foreach (var package in project.NuGetPackages)
            {
                WriteNuGetPackage(xw, package);
            }

            // close item group for nuget packages
            xw.WriteEndElement();

            var pkgConf = new PackagesConfigGenerator().Generate(project.NuGetPackages, project.TargetFrameworks);
            project.AddFile(pkgConf);
        }

        private void WriteNuGetPackage(XmlWriter xw, NuGetPackage package)
        {
            foreach (var assembly in package.Assemblies)
            {
                xw.WriteStartElement("Reference");
                xw.WriteAttributeString("Include", assembly.PublicAssemblyName);

                xw.WriteElementString(
                    "HintPath",
                    Path.Combine(
                        "..",
                        "packages",
                        $"{package.Name}.{package.Version}",
                        "lib",
                        assembly.RelativeHintPath));

                xw.WriteEndElement();
            }
        }

        private void WriteProjectNuGetPackageImports(XmlWriter xw, Project project)
        {
            if (project.NuGetPackages.Count <= 0) return;

            foreach (var package in project.NuGetPackages)
            {
                string packagePath =
                    Path.Combine(
                        "..",
                        "packages",
                        $"{package.Name}.{package.Version}");

                string targetsFile = Path.Combine(packagePath, "build", $"{package.Name}.targets");
                string propsFile = Path.Combine(packagePath, "build", $"{package.Name}.props");

                xw.WriteStartElement("Import");
                xw.WriteAttributeString("Project", propsFile);
                xw.WriteAttributeString("Condition", $"Exists('{propsFile}')");
                xw.WriteEndElement();

                xw.WriteStartElement("Import");
                xw.WriteAttributeString("Project", targetsFile);
                xw.WriteAttributeString("Condition", $"Exists('{targetsFile}')");
                xw.WriteEndElement();
            }
        }

        private void WriteProjectFiles(XmlWriter xw, Project project, string projectRootPath)
        {
            if (project.Files.Count <= 0) return;

            // open item group for files
            xw.WriteStartElement("ItemGroup");

            // write project files
            var fileWriter = new ProjectFileWriter();
            foreach (var file in project.Files)
            {
                xw.WriteStartElement("Compile");
                xw.WriteAttributeString("Include", file.Path);
                xw.WriteEndElement();
                fileWriter.Write(file, projectRootPath);
            }

            // close item group for files
            xw.WriteEndElement();
        }
    }
}
