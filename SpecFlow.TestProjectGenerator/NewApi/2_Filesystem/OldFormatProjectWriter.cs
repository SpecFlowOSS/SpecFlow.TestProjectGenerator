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

            var xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true,
                NewLineOnAttributes = false,
                Encoding = Encoding.UTF8,
            };

            using (var xw = XmlTextWriter.Create(projFilePath, xmlWriterSettings))
            {
                xw.WriteStartDocument();

                // project tag
                xw.WriteStartElement(string.Empty, "Project", msbuildNamespace);
                xw.WriteAttributeString("ToolsVersion", "15.0");
                xw.WriteAttributeString("DefaultTargets", "Build");

                WriteNuGetPackagePropsImports(xw, project);

                WriteProjectProperties(xw, project);
                WriteProjectReferences(xw, project);
                WriteProjectNuGetPackages(xw, project);
                WriteProjectFiles(xw, project, path);

                WriteMSBuildImport(xw, "$(MSBuildToolsPath)\\Microsoft.CSharp.targets");

                WriteNuGetPackageTargetImports(xw, project);

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
            xw.WriteElementString("Configuration", "Debug");
            xw.WriteElementString("Platform", "AnyCPU");
            xw.WriteElementString("ProductVersion", null);
            xw.WriteElementString("SchemaVersion", "2.0");
            xw.WriteElementString("ProjectGuid", project.ProjectGuid.ToString("B"));
            xw.WriteElementString("AppDesignerFolder", "Properties");
            xw.WriteElementString("ProjectTypeGuids", "{3AC096D0-A1C2-E12C-1390-A8335801FDAB};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
            xw.WriteElementString("ShowTrace", "true");
            xw.WriteElementString("OutputType", outputType);
            xw.WriteElementString("RootNamespace", project.Name);
            xw.WriteElementString("AssemblyName", project.Name);
            xw.WriteElementString("FileAlignment", "512");
            xw.WriteElementString("TargetFrameworkVersion", targetFramework);


            xw.WriteElementString("DebugSymbols", "true");
            xw.WriteElementString("DebugType", "full");
            xw.WriteElementString("Optimize", "false");
            xw.WriteElementString("OutputPath", "bin\\Debug");
            xw.WriteElementString("DefineConstants", "DEBUG;TRACE");
            xw.WriteElementString("ErrorReport", "prompt");
            xw.WriteElementString("WarningLevel", "4");

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

                xw.WriteElementString("HintPath", Path.Combine("..","packages",$"{package.Name}.{package.Version}","lib",assembly.RelativeHintPath));

                xw.WriteEndElement();
            }
        }

        private void WriteNuGetPackageTargetImports(XmlWriter xw, Project project)
        {
            WriteNuGetPackageMSBuildImports(xw, project, "targets");
        }

        private void WriteNuGetPackagePropsImports(XmlWriter xw, Project project)
        {
            WriteNuGetPackageMSBuildImports(xw, project, "props");
        }

        private void WriteNuGetPackageMSBuildImports(XmlWriter xw, Project project, string extension)
        {
            if (project.NuGetPackages.Count <= 0) return;

            foreach (var package in project.NuGetPackages)
            {
                string packagePath = Path.Combine("..", "packages", $"{package.Name}.{package.Version}");

                string targetsFile = Path.Combine(packagePath, "build", $"{package.Name}.{extension}");


                WriteMSBuildImport(xw, targetsFile);
            }
        }

        private void WriteMSBuildImport(XmlWriter xw, string file)
        {
            xw.WriteStartElement("Import");
            xw.WriteAttributeString("Project", file);
            xw.WriteAttributeString("Condition", $"Exists('{file}')");
            xw.WriteEndElement();
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
                xw.WriteStartElement(file.BuildAction);
                xw.WriteAttributeString("Include", file.Path);
                xw.WriteEndElement();
                fileWriter.Write(file, projectRootPath);
            }

            // close item group for files
            xw.WriteEndElement();
        }
    }
}