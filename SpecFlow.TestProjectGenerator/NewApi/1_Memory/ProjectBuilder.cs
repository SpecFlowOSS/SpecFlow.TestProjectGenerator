using System;
using System.Collections.Generic;
using System.IO;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi.Driver;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationModel;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectBuilder
    {
        protected readonly TestProjectFolders _testProjectFolders;
        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly ConfigurationGeneratorFactory _configurationGeneratorFactory;
        protected readonly CurrentVersionDriver _currentVersionDriver;
        private readonly Folders _folders;
        private Project _project;
        private bool _parallelTestExecution;

        public ProjectBuilder(TestProjectFolders testProjectFolders, FeatureFileGenerator featureFileGenerator, BindingsGeneratorFactory bindingsGeneratorFactory, ConfigurationGeneratorFactory configurationGeneratorFactory, Configuration configuration, CurrentVersionDriver currentVersionDriver, Folders folders)
        {
            _testProjectFolders = testProjectFolders;
            _featureFileGenerator = featureFileGenerator;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _configurationGeneratorFactory = configurationGeneratorFactory;
            Configuration = configuration;
            _currentVersionDriver = currentVersionDriver;
            _folders = folders;
            ProjectName = $"TestProject_{ProjectGuid:N}";
        }

        public void AddProjectReference(string projectPath, ProjectBuilder projectToReference)
        {
            EnsureProjectExists();
            _project.AddProjectReference(projectPath, projectToReference);
        }

        public Guid ProjectGuid { get; } = Guid.NewGuid();
        public Configuration Configuration { get; }
        public string ProjectName { get; set; }
        public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.CSharp;
        public TargetFramework TargetFrameworks { get; set; } = TargetFramework.Net452;
        public ProjectFormat Format { get; set; } = ProjectFormat.Old;
        public ConfigurationFormat ConfigurationFormat { get; set; } = ConfigurationFormat.Config;

        public bool IsSpecFlowFeatureProject { get; set; } = true;

        public void AddFile(ProjectFile projectFile)
        {
            EnsureProjectExists();
            _project.AddFile(projectFile ?? throw new ArgumentNullException(nameof(projectFile)));
        }

        public void AddFeatureFile(string featureFileContent)
        {
            EnsureProjectExists();

            var featureFile = _featureFileGenerator.Generate(featureFileContent);

            _project.AddFile(featureFile);
        }

        public void AddStepBinding(string attributeName, string regex, string csharpcode, string vbnetcode)
        {
            EnsureProjectExists();

            string methodImplementation = GetCode(_project.ProgrammingLanguage, csharpcode, vbnetcode);
            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);

            _project.AddFile(bindingsGenerator.GenerateStepDefinition("StepBinding", methodImplementation, attributeName, regex));
            _project.AddFile(bindingsGenerator.GenerateStepDefinition("StepBinding", methodImplementation, attributeName, regex, ParameterType.Table, "tableArg"));
            _project.AddFile(bindingsGenerator.GenerateStepDefinition("StepBinding", methodImplementation, attributeName, regex, ParameterType.DocString, "docStringArg"));
        }

        public void AddLoggingStepBinding(string attributeName, string methodName, string pathToLogFile, string regex)
        {
            EnsureProjectExists();
            
            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);

            _project.AddFile(bindingsGenerator.GenerateLoggingStepDefinition(methodName, pathToLogFile, attributeName, regex));
            _project.AddFile(bindingsGenerator.GenerateLoggingStepDefinition(methodName, pathToLogFile, attributeName, regex, ParameterType.Table, "tableArg"));
            _project.AddFile(bindingsGenerator.GenerateLoggingStepDefinition(methodName, pathToLogFile, attributeName, regex, ParameterType.DocString, "docStringArg"));
        }

        public void AddHookBinding(string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null, IList<string> classScopeAttributeTags = null)
        {
            EnsureProjectExists();

            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);
            _project.AddFile(bindingsGenerator.GenerateHookBinding(eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags));
        }

        public void AddStepBinding(string bindingCode)
        {
            EnsureProjectExists();

            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);
            _project.AddFile(bindingsGenerator.GenerateStepDefinition(bindingCode));
        }

        public void AddBindingClass(string fullBindingClass)
        {
            EnsureProjectExists();

            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);
            var replacedBindingClass = fullBindingClass.Replace("$ProjectDir$", _testProjectFolders.ProjectFolder);
            _project.AddFile(bindingsGenerator.GenerateBindingClassFile(replacedBindingClass));
        }

        public void GenerateConfigurationFile()
        {
            EnsureProjectExists();
            var generator = _configurationGeneratorFactory.FromConfigurationFormat(ConfigurationFormat);
            _project.AddFile(generator.Generate(Configuration));
        }

        public Project Build()
        {
            EnsureProjectExists();
            return _project;
        }

        private string GetCode(ProgrammingLanguage language, string csharpcode, string vbnetcode)
        {
            switch (language)
            {
                case ProgrammingLanguage.CSharp:
                    return csharpcode;
                case ProgrammingLanguage.VB:
                    return vbnetcode;
                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, null);
            }
        }

        private void AddInitialFSharpReferences()
        {
            switch (_project.ProjectFormat)
            {
                case ProjectFormat.Old:
                    AddInitialOldFormatFSharpReferences();
                    break;
                case ProjectFormat.New:
                    AddInitialNewFormatFSharpReferences();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void AddInitialOldFormatFSharpReferences()
        {
            _project.AddNuGetPackage("FSharp.Compiler.Tools", "10.0.2");
            _project.AddReference("FSharp.Core");
            _project.AddReference("System.Numerics");
        }

        private void AddInitialNewFormatFSharpReferences()
        {
        }

        private void EnsureProjectExists()
        {
            if (_project != null)
            {
                return;
            }

            _project = new Project(ProjectName, ProjectGuid, Language, TargetFrameworks, Format);


            _testProjectFolders.ProjectFolder = Path.Combine(_testProjectFolders.PathToSolutionDirectory, _project.Name);
            _testProjectFolders.ProjectBinOutputPath = Path.Combine(_testProjectFolders.ProjectFolder, GetProjectCompilePath(_project));
            _testProjectFolders.TestAssemblyFileName = $"{_project.Name}.dll";
            _testProjectFolders.PathToNuGetPackages = _project.ProjectFormat == ProjectFormat.Old  ? Path.Combine(_testProjectFolders.PathToSolutionDirectory, "packages") : _folders.GlobalPackages;
            _testProjectFolders.CompiledAssemblyPath = Path.Combine(_testProjectFolders.ProjectBinOutputPath, _testProjectFolders.TestAssemblyFileName);

            _project.AddNuGetPackage("BoDi", "1.4.0-alpha1", new NuGetPackageAssembly("BoDi, Version=1.4.0.0, Culture=neutral, PublicKeyToken=ff7cd5ea2744b496", "net45\\BoDi.dll"));
            _project.AddNuGetPackage("SpecFlow", _currentVersionDriver.SpecFlowNuGetVersion, new NuGetPackageAssembly(GetSpecFlowPublicAssemblyName("TechTalk.SpecFlow"), "net45\\TechTalk.SpecFlow.dll"));

            var generator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);
            _project.AddFile(generator.GenerateLoggerClass(Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log")));

            switch (_project.ProgrammingLanguage)
            {
                case ProgrammingLanguage.FSharp:
                    AddInitialFSharpReferences();
                    break;
                case ProgrammingLanguage.CSharp:
                    AddUnitTestProviderSpecificConfig();
                    break;
            }

            if (IsSpecFlowFeatureProject)
            {
                if (_currentVersionDriver.SpecFlowVersion >= new Version(2, 3, 0))
                {
                    _project.AddNuGetPackage("SpecFlow.Tools.MsBuild.Generation", _currentVersionDriver.SpecFlowNuGetVersion);
                }
                else
                {
                    _project.AddMSBuildImport($"..\\packages\\SpecFlow.{_currentVersionDriver.SpecFlowVersion}\\tools\\TechTalk.SpecFlow.targets");
                }

                if (_currentVersionDriver.SpecFlowVersion < new Version(3, 0, 0))
                {
                    AddMSBuildTarget("AfterUpdateFeatureFilesInProject", @"<ItemGroup>
                                                                          <Compile Include=""@(SpecFlowGeneratedFiles)"" />
                                                                       </ItemGroup>");
                }
            }

            switch (Configuration.UnitTestProvider)
            {
                case UnitTestProvider.SpecRun:
                    ConfigureRunner();
                    break;
                case UnitTestProvider.SpecRunWithNUnit:
                    ConfigureRunner();
                    ConfigureNUnit();
                    break;
                case UnitTestProvider.SpecRunWithMsTest:
                    ConfigureRunner();
                    ConfigureMSTest();
                    break;
                case UnitTestProvider.MSTest:
                    ConfigureMSTest();
                    break;
                case UnitTestProvider.XUnit:
                    ConfigureXUnit();
                    break;
                case UnitTestProvider.NUnit3:
                    ConfigureNUnit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _project.AddNuGetPackage("Newtonsoft.Json", "11.0.2");
            _project.AddNuGetPackage("FluentAssertions", "5.3.0");
            AddAdditionalStuff();
        }

        private void ConfigureNUnit()
        {
            _project.AddNuGetPackage("NUnit", "3.8.1");
            _project.AddNuGetPackage("NUnit3TestAdapter", "3.8.0");
            

            if (_currentVersionDriver.SpecFlowVersion >= new Version(3, 0))
            {
                _project.AddNuGetPackage("SpecFlow.NUnit", _currentVersionDriver.SpecFlowNuGetVersion, new NuGetPackageAssembly(GetSpecFlowPublicAssemblyName("TechTalk.SpecFlow.NUnit.SpecFlowPlugin.dll"), "net45\\TechTalk.SpecFlow.NUnit.SpecFlowPlugin.dll"));
                Configuration.Plugins.Add(new SpecFlowPlugin("TechTalk.SpecFlow.NUnit", SpecFlowPluginType.Runtime));
            }
        }

        private void ConfigureXUnit()
        {
            _project.AddNuGetPackage("xunit.core", "2.3.1");
            _project.AddNuGetPackage("xunit.extensibility.core", "2.3.1", new NuGetPackageAssembly("xunit.core, Version=2.3.1.3858, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.1\\xunit.core.dll"));
            _project.AddNuGetPackage("xunit.extensibility.execution", "2.3.1",
                new NuGetPackageAssembly("xunit.execution.desktop, Version=2.3.1.3858, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "net452\\xunit.execution.desktop.dll"));
            _project.AddNuGetPackage("xunit.assert", "2.3.1", new NuGetPackageAssembly("xunit.assert, Version=2.3.1.3858, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.1\\xunit.assert.dll"));
            _project.AddNuGetPackage("xunit.abstractions", "2.0.1", new NuGetPackageAssembly("xunit.abstractions, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.0\\xunit.abstractions.dll"));
            _project.AddNuGetPackage("xunit.runner.visualstudio", "2.3.1");

            if (_currentVersionDriver.SpecFlowVersion >= new Version(3, 0))
            {
                _project.AddNuGetPackage("SpecFlow.xUnit", _currentVersionDriver.SpecFlowNuGetVersion, new NuGetPackageAssembly(GetSpecFlowPublicAssemblyName("TechTalk.SpecFlow.xUnit.SpecFlowPlugin.dll"), "net45\\TechTalk.SpecFlow.xUnit.SpecFlowPlugin.dll"));
                Configuration.Plugins.Add(new SpecFlowPlugin("TechTalk.SpecFlow.xUnit", SpecFlowPluginType.Runtime));
            }
        }

        private void ConfigureMSTest()
        {
            _project.AddNuGetPackage("MSTest.TestAdapter", "1.3.0");
            _project.AddNuGetPackage(
                "MSTest.TestFramework",
                "1.3.0",
                new NuGetPackageAssembly(
                    "Microsoft.VisualStudio.TestPlatform.TestFramework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL",
                    "net45\\Microsoft.VisualStudio.TestPlatform.TestFramework.dll"),
                new NuGetPackageAssembly(
                    "Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL",
                    "net45\\Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll"));

            if (_currentVersionDriver.SpecFlowVersion >= new Version(3, 0))
            {
                _project.AddNuGetPackage("SpecFlow.MSTest", _currentVersionDriver.SpecFlowNuGetVersion, new NuGetPackageAssembly(GetSpecFlowPublicAssemblyName("TechTalk.SpecFlow.MSTest.SpecFlowPlugin.dll"), "net45\\TechTalk.SpecFlow.MSTest.SpecFlowPlugin.dll"));
                Configuration.Plugins.Add(new SpecFlowPlugin("TechTalk.SpecFlow.MSTest", SpecFlowPluginType.Runtime));
            }
        }

        private void ConfigureRunner()
        {
            _project.AddNuGetPackage("SpecRun.Runner", _currentVersionDriver.NuGetVersion);
            _project.AddNuGetPackage($"SpecRun.SpecFlow.{_currentVersionDriver.SpecFlowVersionDash}", _currentVersionDriver.NuGetVersion,
                new NuGetPackageAssembly($"SpecRun.SpecFlowPlugin, Version={_currentVersionDriver.MajorMinorPatchVersion}.0, Culture=neutral, processorArchitecture=MSIL", "net45\\SpecRun.SpecFlowPlugin.dll"),
                new NuGetPackageAssembly($"TechTalk.SpecRun, Version={_currentVersionDriver.MajorMinorPatchVersion}.0, Culture=neutral, PublicKeyToken=d0fc5cc18b3b389b, processorArchitecture=MSIL",
                    "net45\\TechTalk.SpecRun.dll"),
                new NuGetPackageAssembly($"TechTalk.SpecRun.Common, Version={_currentVersionDriver.MajorMinorPatchVersion}.0, Culture=neutral, PublicKeyToken=d0fc5cc18b3b389b, processorArchitecture=MSIL",
                    "net45\\TechTalk.SpecRun.Common.dll")
            );
            Configuration.Plugins.Add(new SpecFlowPlugin("SpecRun"));
        }

        protected virtual void AddAdditionalStuff()
        {
            
        }

        private void AddUnitTestProviderSpecificConfig()
        {
            switch (Configuration.UnitTestProvider)
            {
                case UnitTestProvider.XUnit when !_parallelTestExecution:
                    _project.AddFile(new ProjectFile("XUnitConfiguration.cs", "Compile", "using Xunit; [assembly: CollectionBehavior(MaxParallelThreads = 1, DisableTestParallelization = true)]"));
                    break;
                case UnitTestProvider.XUnit:
                    _project.AddFile(new ProjectFile("XUnitConfiguration.cs", "Compile", "using Xunit; [assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, MaxParallelThreads = 4)]"));
                    break;
                case UnitTestProvider.NUnit3 when _parallelTestExecution:
                    _project.AddFile(new ProjectFile("NUnitConfiguration.cs", "Compile", "[assembly: NUnit.Framework.Parallelizable(NUnit.Framework.ParallelScope.Fixtures)]"));
                    break;
                case UnitTestProvider.MSTest when _parallelTestExecution:
                    _project.AddFile(
                        new ProjectFile("MsTestConfiguration.cs", "Compile", "using Microsoft.VisualStudio.TestTools.UnitTesting; [assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]"));
                    break;
                case UnitTestProvider.MSTest when !_parallelTestExecution:
                    _project.AddFile(new ProjectFile("MsTestConfiguration.cs", "Compile", "using Microsoft.VisualStudio.TestTools.UnitTesting; [assembly: DoNotParallelize]"));
                    break;
            }
        }

        private string GetSpecFlowPublicAssemblyName(string assemblyName)
        {
#if SPECFLOW_ENABLE_STRONG_NAME_SIGNING
            return $"{assemblyName}, Version={_currentVersionDriver.SpecFlowVersion}.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL";
#else
            return assemblyName;
#endif
        }

        public void EnableParallelTestExecution()
        {
            _parallelTestExecution = true;
        }

        private string GetProjectCompilePath(Project project)
        {
            // TODO: hardcoded "Debug" value should be replaced by a configuration parameter
            if (project.ProjectFormat == ProjectFormat.New)
            {
                return Path.Combine("bin", "Debug", project.TargetFrameworks.ToTargetFrameworkMoniker().Split(';')[0]);
            }

            return Path.Combine("bin", "Debug");
        }

        public void AddMSBuildTarget(string targetName, string implementation)
        {
            _project.AddTarget(targetName, implementation);
        }

        public void AddNuGetPackage(string nugetPackage, string nugetVersion)
        {
            _project.AddNuGetPackage(nugetPackage, nugetVersion);
        }

    }
}
