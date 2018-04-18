using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;
using SpecFlow.TestProjectGenerator.Helpers;
using SpecFlow.TestProjectGenerator.Inputs;
using SpecFlow.TestProjectGenerator.ProgramLanguageDrivers;

namespace SpecFlow.TestProjectGenerator
{
    public class ProjectCompiler
    {
        protected readonly Folders _folders;
        protected readonly ProjectCompilerHelper _projectCompilerHelper;
        private readonly CurrentVersionDriver _currentVersionDriver;
        protected readonly VisualStudioFinder _visualStudioFinder;
        protected IProgramLanguageProjectCompiler _programLanguageProjectCompiler;

        public ProjectCompiler(Folders folders, VisualStudioFinder visualStudioFinder, ProjectCompilerHelper projectCompilerHelper, CurrentVersionDriver currentVersionDriver)
        {
            _folders = folders;
            _visualStudioFinder = visualStudioFinder;
            _projectCompilerHelper = projectCompilerHelper;
            _currentVersionDriver = currentVersionDriver;
        }

        public void Compile(InputProjectDriver inputProjectDriver)
        {
            //switch für programmiersprachen
            _programLanguageProjectCompiler = GetProgramLanguageProjectCompiler(inputProjectDriver);

            Console.WriteLine("Compiling project '{0}' in '{1}'", inputProjectDriver.ProjectFileName, inputProjectDriver.ProjectFolder);

            EnsureCompilationFolder(inputProjectDriver.ProjectFolder);


            AddNugetStuff(inputProjectDriver);
            RestoreNugetPackage(inputProjectDriver);


            Project project = CreateProject(inputProjectDriver, inputProjectDriver.ProjectFileName);


            project.AddItem("None", "packages.config");
            project.AddItem("None", "..\\NuGet.config");

            AddAdditionalStuff(inputProjectDriver, project);


            AddAppConfig(inputProjectDriver, project);
            AddMsTestTestSettings(inputProjectDriver, project);


            


            var references = new List<string>(inputProjectDriver.AdditionalReferences);
            if (inputProjectDriver.UnitTestProvider == UnitTestProvider.SpecRunWithNUnit || inputProjectDriver.UnitTestProvider == UnitTestProvider.SpecRunWithNUnit2 || inputProjectDriver.UnitTestProvider == UnitTestProvider.NUnit3 || inputProjectDriver.UnitTestProvider == UnitTestProvider.NUnit2)
            {
                references.Add(Path.Combine(_folders.TestFolder, "nunit.framework.dll"));
            }

            foreach (var referencePath in references)
            {
                project.AddItem("Reference", Path.GetFileNameWithoutExtension(referencePath),
                    new Dictionary<string, string>()
                    {
                        {"HintPath", Path.Combine(_folders.TestFolder, referencePath)}
                    });
            }

            foreach (var bindingClassInput in inputProjectDriver.BindingClasses)
            {
                AddBindingClass(inputProjectDriver, project, bindingClassInput);
            }

            foreach (var featureFileInput in inputProjectDriver.FeatureFiles)
            {
                string outputPath = Path.Combine(inputProjectDriver.ProjectFolder, featureFileInput.ProjectRelativePath);
                WriteOutInputFile(featureFileInput, outputPath);
                var generatedFile = featureFileInput.ProjectRelativePath + _programLanguageProjectCompiler.FileEnding;
                project.AddItem("None", featureFileInput.ProjectRelativePath, new[]
                {
                    new KeyValuePair<string, string>("Generator", "SpecFlowSingleFileGenerator"),
                    new KeyValuePair<string, string>("LastGenOutput", generatedFile),
                });
                project.AddItem("Compile", generatedFile);
            }

            foreach (var codeFileInput in inputProjectDriver.CodeFileInputs)
            {
                string outputPath = Path.Combine(inputProjectDriver.ProjectFolder, codeFileInput.ProjectRelativePath);
                WriteOutInputFile(codeFileInput, outputPath);

                project.AddItem("Compile", outputPath);
            }


            foreach (var contentFileInput in inputProjectDriver.ContentFiles)
            {
                if (contentFileInput.Content != null)
                {
                    _projectCompilerHelper.SaveFileFromTemplate(inputProjectDriver.ProjectFolder, contentFileInput.Content, contentFileInput.ProjectRelativePath);
                }
                else
                {
                    string outputPath = Path.Combine(inputProjectDriver.ProjectFolder, contentFileInput.ProjectRelativePath);
                    File.Copy(contentFileInput.SourceFilePath, outputPath, true);
                }

                project.AddItem("Content", contentFileInput.ProjectRelativePath, new[]
                {
                    new KeyValuePair<string, string>("CopyToOutputDirectory", "PreserveNewest"),
                });
            }

            _programLanguageProjectCompiler.AdditionalAdjustments(project, inputProjectDriver);


            if (_folders.VsAdapterFolderChanged)
            {
                CopyVSTestAdapter(_folders.VsAdapterFolderProjectBinaries, _folders.VSAdapterFolder);
            }

            project.Save();

            
            CompileOutProc(project, inputProjectDriver);

            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
        }

        protected virtual void AddAdditionalStuff(InputProjectDriver inputProjectDriver, Project project)
        {
        }

        private void CopyVSTestAdapter(string foldersVsAdapterFolderProjectBinaries, string foldersVsAdapterFolder)
        {
            FileSystemHelper.CopyDirectory(foldersVsAdapterFolderProjectBinaries, foldersVsAdapterFolder, true);
        }


        private void RestoreNugetPackage(InputProjectDriver inputProjectDriver)
        {
            var processPath = Path.Combine(_folders.GlobalPackages, "NuGet.CommandLine","4.5.1", "tools", "NuGet.exe");

            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException("NuGet.exe could not be found! Is the version number correct?", processPath);
            }

            var commandLineArgs = $"restore Project\\packages.config -SolutionDirectory . ";


            var nugetRestore = new ProcessHelper();
            var processResult = nugetRestore.RunProcess(inputProjectDriver.SolutionFolder, processPath, commandLineArgs);

            if (processResult.ExitCode > 0)
            {
                throw new Exception("NuGet restore failed - rebuild solution to generate latest packages " + Environment.NewLine +
                                    $"{inputProjectDriver.SolutionFolder} {processPath} {commandLineArgs}" + Environment.NewLine + processResult.CombinedOutput);
            }
        }


        private IProgramLanguageProjectCompiler GetProgramLanguageProjectCompiler(InputProjectDriver inputProjectDriver)
        {
            switch (inputProjectDriver.ProgrammingLanguage)
            {
                case ProgrammingLanguage.CSharp:
                    return new CSharpProgramLanguageProjectCompiler(_projectCompilerHelper, _currentVersionDriver);
                case ProgrammingLanguage.VB:
                    return new VBNetProjectCompiler(_projectCompilerHelper);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddNugetStuff(InputProjectDriver inputProjectDriver)
        {
            _projectCompilerHelper.SaveFileFromResourceTemplate(inputProjectDriver.SolutionFolder, "NuGet.config", "NuGet.config", new Dictionary<string, string>()
            {
                {"FeedPath", _folders.NuGetFolder}
            });
            
            _projectCompilerHelper.SaveFileFromResourceTemplate(inputProjectDriver.ProjectFolder, "packages.config", "packages.config", new Dictionary<string, string>()
            {
                {"NuGetVersion", _currentVersionDriver.NuGetVersion},
                {"TestingFrameworkPackage", inputProjectDriver.TestingFrameworkPackage}
            });

        }


        private static void WriteOutInputFile(FileInputWithContent inputFile, string outputPath)
        {
            if (inputFile.SourceFilePath != null)
                File.Copy(inputFile.SourceFilePath, outputPath, true);
            else
                File.WriteAllText(outputPath, inputFile.Content, Encoding.UTF8);
        }

        private void CompileOutProc(Project project, InputProjectDriver inputProjectDriver)
        {
            string msBuildPath = _visualStudioFinder.FindMSBuild();
            Console.WriteLine("Invoke MsBuild from {0}", msBuildPath);


            var processHelper = new ProcessHelper();
            var msBuildExitCode = processHelper.RunProcess(inputProjectDriver.SolutionFolder, msBuildPath, $"/bl /nologo /v:m \"{inputProjectDriver.SolutionPath}\"");

            
            if (msBuildExitCode.ExitCode > 0)
            {
                var firstErrorLine = msBuildExitCode.CombinedOutput.SplitByString(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(l => l.Contains("): error "));
                throw new Exception($"Build failed: {firstErrorLine}");
            }
        }

        private void AddBindingClass(InputProjectDriver inputProjectDriver, Project project, BindingClassInput bindingClassInput)
        {
            _programLanguageProjectCompiler.AddBindingClass(inputProjectDriver, project, bindingClassInput);
        }

        private void AddAppConfig(InputProjectDriver inputProjectDriver, Project project)
        {
            var replacements = new Dictionary<string, string>
            {
                {"UnitTestProvider", GetUnitTestProvider(inputProjectDriver)},
                {"AdditionalSpecFlowPlugins", inputProjectDriver.AdditionalSpecFlowPlugins},
                {"AdditionalSpecFlowSettings", inputProjectDriver.AdditionalSpecFlowSettings},
                {"BindingRedirects", inputProjectDriver.BindingRedirects }
            };
            _projectCompilerHelper.SaveFileFromResourceTemplate(inputProjectDriver.ProjectFolder, "App.config", "App.config", replacements);
            project.AddItem("None", "App.config");
        }

        private void AddMsTestTestSettings(InputProjectDriver inputProjectDriver, Project project)
        {
            _projectCompilerHelper.SaveFileFromResourceTemplate(inputProjectDriver.ProjectFolder, "local.testsettings", "local.testsettings", new Dictionary<string, string>());
            project.AddItem("None", "local.testsettings");

            var replacements = new Dictionary<string, string>() {{"VSAdapterPath", _folders.VSAdapterFolder}};
            _projectCompilerHelper.SaveFileFromResourceTemplate(inputProjectDriver.ProjectFolder, "local.runsettings", "local.runsettings", replacements);
            project.AddItem("None", "local.runsettings");
        }


        private string GetUnitTestProvider(InputProjectDriver inputProjectDriver)
        {
            switch (inputProjectDriver.UnitTestProvider)
            {
                case UnitTestProvider.SpecRunWithNUnit:
                    return "SpecRun+NUnit";
                case UnitTestProvider.SpecRunWithNUnit2:
                    return "SpecRun+NUnit.2";
                case UnitTestProvider.SpecRunWithMsTest:
                    return "SpecRun+MsTest";
                case UnitTestProvider.MSTest:
                    return "MSTest";
                case UnitTestProvider.NUnit2:
                    return "NUnit2";
                case UnitTestProvider.NUnit3:
                    return "NUnit";
                case UnitTestProvider.XUnit:
                    return "XUnit";
                default:
                    return "SpecRun";
            }
        }


        private Project CreateProject(InputProjectDriver inputProjectDriver, string outputFileName)
        {
            // the MsBuild global collection caches the project file, so we need to generate a unique project file name.
            Guid projectId = inputProjectDriver.ProjectGuid;

            string solutionFileName = _projectCompilerHelper.SaveFileFromResourceTemplate(inputProjectDriver.SolutionFolder, "TestProjectSolution.sln", inputProjectDriver.SolutionFileName,
                new Dictionary<string, string>()
                {
                    {"ProjectGuid", projectId.ToString("B").ToUpper()},
                    {"ProjectName", Path.GetFileNameWithoutExtension(outputFileName)},
                    {"ProjectFileName", inputProjectDriver.ProjectPath}
                });

            string projectFileName = _projectCompilerHelper.SaveFileFromResourceTemplate(inputProjectDriver.ProjectFolder, _programLanguageProjectCompiler.ProjectFileName, outputFileName, new Dictionary<string, string>
            {
                {"ProjectGuid", projectId.ToString("B")},
                {"ProjectName", Path.GetFileNameWithoutExtension(outputFileName)},
                {"NetFrameworkVersion", inputProjectDriver.NetFrameworkVersion},
                {"TestingFramework", inputProjectDriver.TestingFrameworkReference},
                {"PreMsBuild", inputProjectDriver.PreMsBuild },
                {"PostMsBuild", inputProjectDriver.PostMsBuild }
            });


            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
            return new Project(projectFileName);
        }

        private void EnsureCompilationFolder(string compilationFolder)
        {
            FileSystemHelper.EnsureEmptyFolder(compilationFolder);
        }
    }
}