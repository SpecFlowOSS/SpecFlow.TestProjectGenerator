using System;
using System.IO;
using System.Linq;
using System.Xml;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class OldFormatProjectWriter : XmlFileGeneratorBase, IProjectWriter
    {
        private readonly IOutputWriter _outputWriter;
        private readonly ProjectFileWriter _fileWriter = new ProjectFileWriter();

        public OldFormatProjectWriter(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
        }

        public virtual string WriteProject(Project project, string path)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            Directory.CreateDirectory(path);
            string projFileName = $"{project.Name}.{project.ProgrammingLanguage.ToProjectFileExtension()}";
            string projFilePath = Path.Combine(path, projFileName);

            const string msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

            using (var xw = GenerateDefaultXmlWriter(projFilePath))
            {
                xw.WriteStartDocument();

                // project tag
                xw.WriteStartElement(string.Empty, "Project", msbuildNamespace);
                xw.WriteAttributeString("ToolsVersion", "15.0");
                xw.WriteAttributeString("DefaultTargets", "Build");

                WriteNuGetPackagePropsImports(xw, project);

                WriteProjectProperties(xw, project);
                WriteReferences(xw, project);
                WriteProjectReferences(xw, project);
                WriteProjectNuGetPackages(xw, project);
                WriteProjectFiles(xw, project, path);

                WriteLanguageTargets(xw, project);

                

                WriteNuGetPackageTargetImports(xw, project);

                WriteMSBuildImports(xw, project);
                WriteMSBuildTargets(xw, project);
                

                // close project tag
                xw.WriteEndElement();
                xw.WriteEndDocument();
            }

            return projFilePath;
        }

        private void WriteMSBuildImports(XmlWriter xmlWriter, Project project)
        {
            foreach (var msBuildImport in project.MSBuildImports)
            {
                WriteMSBuildImport(xmlWriter, msBuildImport.MsbuildTargetFile);
            }
        }

        private void WriteMSBuildTargets(XmlWriter xmlWriter, Project project)
        {
            foreach (var msBuildTarget in project.MSBuildTargets)
            {
                xmlWriter.WriteStartElement("Target");
                xmlWriter.WriteAttributeString("Name", msBuildTarget.Name);

                xmlWriter.WriteRaw(msBuildTarget.Implementation);

                xmlWriter.WriteEndElement();
            }

             
        }

        public void WriteReferences(Project project, string projectFilePath)
        {
            
        }

        private void WriteLanguageTargets(XmlWriter xw, Project project)
        {
            switch (project.ProgrammingLanguage)
            {
                case ProgrammingLanguage.CSharp:
                    WriteMSBuildImport(xw, "$(MSBuildToolsPath)\\Microsoft.CSharp.targets");
                    break;
                case ProgrammingLanguage.FSharp:
                    WriteMSBuildImport(xw, "$(FSharpTargetsPath)");
                    break;
                case ProgrammingLanguage.VB:
                    WriteMSBuildImport(xw, "$(MSBuildToolsPath)\\Microsoft.VisualBasic.targets");
                    break;

                default: throw new NotSupportedException($"The programming language {project.ProgrammingLanguage} is not supported.");
            }
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

            xw.WriteStartElement("PropertyGroup");

            xw.WriteElementString("Configuration", "Debug");
            xw.WriteElementString("Platform", "AnyCPU");
            
            
            xw.WriteElementString("ProjectGuid", project.ProjectGuid.ToString("B"));
            
            if (project.ProgrammingLanguage != ProgrammingLanguage.VB)
            {
                xw.WriteElementString("AppDesignerFolder", "Properties");

                xw.WriteElementString("SchemaVersion", "2.0");
                xw.WriteElementString("ProductVersion", null);
                xw.WriteElementString("ProjectTypeGuids", "{3AC096D0-A1C2-E12C-1390-A8335801FDAB};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
                xw.WriteElementString("ShowTrace", "true");
                xw.WriteElementString("ErrorReport", "prompt");
                xw.WriteElementString("WarningLevel", "4");
            }
            else
            {
                xw.WriteElementString("MyType", "Windows");
                xw.WriteElementString("OptionExplicit", "On");
                xw.WriteElementString("OptionCompare", "Binary");
                xw.WriteElementString("OptionStrict", "Off");
                xw.WriteElementString("OptionInfer", "On");
            }

            xw.WriteElementString("OutputType", outputType);
            xw.WriteElementString("RootNamespace", project.Name);
            xw.WriteElementString("AssemblyName", project.Name);
            xw.WriteElementString("FileAlignment", "512");
            xw.WriteElementString("TargetFrameworkVersion", targetFramework);


            xw.WriteElementString("DebugSymbols", "true");
            xw.WriteElementString("DebugType", "full");
            xw.WriteElementString("Optimize", "false");
            xw.WriteElementString("OutputPath", "bin\\Debug");

            if (project.ProgrammingLanguage == ProgrammingLanguage.VB)
            {
                xw.WriteElementString("DefineDebug", "true");
                xw.WriteElementString("DefineTrace", "true");
            }
            else
            {
                xw.WriteElementString("DefineConstants", "DEBUG;TRACE");
            }


            xw.WriteEndElement();
        }

        private void WriteProjectReferences(XmlWriter xw, Project project)
        {
            if (project.References.Count <= 0) return;

            // item group for project references
            xw.WriteStartElement("ItemGroup");

            foreach (var reference in project.ProjectReferences)
            {
                xw.WriteStartElement("ProjectReference");
                xw.WriteAttributeString("Include", reference.Path);

                xw.WriteElementString("Project", reference.Project.ProjectGuid.ToString("B"));
                xw.WriteElementString("Name", reference.Project.ProjectName);

                xw.WriteEndElement();
            }

            xw.WriteEndElement();
        }

        private void WriteReferences(XmlWriter xw, Project project)
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

            xw.WriteStartElement("ItemGroup");

            foreach (var package in project.NuGetPackages)
            {
                WriteNuGetPackage(xw, package);
            }

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

                xw.WriteElementString("HintPath", Path.Combine("..", "packages", $"{package.Name}.{package.Version}", "lib", assembly.RelativeHintPath));

                xw.WriteEndElement();
            }
        }

        private void WriteNuGetPackageTargetImports(XmlWriter xw, Project project)
        {
            WriteNuGetPackageMsBuildImports(xw, project, "targets");
        }

        private void WriteNuGetPackagePropsImports(XmlWriter xw, Project project)
        {
            WriteNuGetPackageMsBuildImports(xw, project, "props");
        }

        private void WriteNuGetPackageMsBuildImports(XmlWriter xw, Project project, string extension)
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

            xw.WriteStartElement("ItemGroup");

            foreach (var file in project.Files)
            {
                xw.WriteStartElement(file.BuildAction);
                xw.WriteAttributeString("Include", file.Path);

                if (file.CopyToOutputDirectory != CopyToOutputDirectory.DoNotCopy)
                {
                    xw.WriteElementString("CopyToOutputDirectory", file.CopyToOutputDirectory.GetCopyToOutputDirectoryString());
                }

                xw.WriteEndElement();
                _fileWriter.Write(file, projectRootPath);
            }

            xw.WriteEndElement();
        }
    }
}