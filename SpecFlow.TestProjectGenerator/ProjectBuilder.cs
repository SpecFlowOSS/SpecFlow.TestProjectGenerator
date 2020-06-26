﻿using System;
using System.Collections.Generic;
using System.IO;
using TechTalk.SpecFlow.TestProjectGenerator.ConfigurationModel;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;
using TechTalk.SpecFlow.TestProjectGenerator.Factories.BindingsGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.Factories.ConfigurationGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class ProjectBuilder
    {
        public const string NUnit3PackageName = "NUnit";
        public const string NUnit3PackageVersion = "3.11.0";
        public const string NUnit3TestAdapterPackageName = "NUnit3TestAdapter";
        public const string NUnit3TestAdapterPackageVersion = "3.10.0";
        private const string XUnitPackageVersion = "2.4.1";
        private const string MSTestPackageVersion = "2.0.0";
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly ConfigurationGeneratorFactory _configurationGeneratorFactory;
        protected readonly CurrentVersionDriver _currentVersionDriver;
        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly Folders _folders;
        private readonly TargetFrameworkMonikerStringBuilder _targetFrameworkMonikerStringBuilder;
        protected readonly TestProjectFolders _testProjectFolders;
        private bool _parallelTestExecution;
        private Project _project;

        public ProjectBuilder(
            TestProjectFolders testProjectFolders,
            FeatureFileGenerator featureFileGenerator,
            BindingsGeneratorFactory bindingsGeneratorFactory,
            ConfigurationGeneratorFactory configurationGeneratorFactory,
            Configuration configuration,
            CurrentVersionDriver currentVersionDriver,
            Folders folders,
            TargetFrameworkMonikerStringBuilder targetFrameworkMonikerStringBuilder)
        {
            _testProjectFolders = testProjectFolders;
            _featureFileGenerator = featureFileGenerator;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _configurationGeneratorFactory = configurationGeneratorFactory;
            Configuration = configuration;
            _currentVersionDriver = currentVersionDriver;
            _folders = folders;
            _targetFrameworkMonikerStringBuilder = targetFrameworkMonikerStringBuilder;
            var projectGuidString = $"{ProjectGuid:N}".Substring(24);
            ProjectName = $"TestProj_{projectGuidString}";
        }

        public Guid ProjectGuid { get; } = Guid.NewGuid();
        public Configuration Configuration { get; }
        public string ProjectName { get; set; }
        public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.CSharp;
        public TargetFramework TargetFramework { get; set; } = TargetFramework.Netcoreapp31;
        public string TargetFrameworkMoniker => _targetFrameworkMonikerStringBuilder.BuildTargetFrameworkMoniker(TargetFramework);
        public ProjectFormat Format { get; set; } = ProjectFormat.New;
        public ConfigurationFormat ConfigurationFormat { get; set; } = ConfigurationFormat.Json;

        public bool IsSpecFlowFeatureProject { get; set; } = true;

        public bool? IsTreatWarningsAsErrors { get; set; }

        public ProjectType ProjectType { get; set; } = ProjectType.Library;

        public void AddProjectReference(string projectPath, ProjectBuilder projectToReference)
        {
            EnsureProjectExists();
            _project.AddProjectReference(projectPath, projectToReference);
        }

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

            var methodImplementation = GetCode(_project.ProgrammingLanguage, csharpcode, vbnetcode);
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

        public void AddHookBinding(string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null,
            IList<string> classScopeAttributeTags = null)
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

        public void GenerateSpecFlowConfigurationFile()
        {
            if (!IsSpecFlowFeatureProject)
            {
                return;
            }

            EnsureProjectExists();
            var generator = _configurationGeneratorFactory.FromConfigurationFormat(ConfigurationFormat);
            var generatedConfig = generator.Generate(Configuration);

            if (generatedConfig is ProjectFile _)
            {
                _project.AddFile(generatedConfig);
            }
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
            _project.AddNuGetPackage("FSharp.Compiler.Tools", "10.2.1");
            _project.AddNuGetPackage("FSharp.Core", "4.6.2", new NuGetPackageAssembly("FSharp.Core, Version=4.6.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "net45\\FSharp.Core.dll"));
            _project.AddReference("System.Numerics");
        }

        private void AddInitialNewFormatFSharpReferences()
        {
        }

        private void EnsureProjectExists()
        {
            if (_project != null) return;

            _project = new Project(ProjectName, ProjectGuid, Language, TargetFramework, Format, ProjectType);

            _testProjectFolders.PathToNuGetPackages = _project.ProjectFormat == ProjectFormat.Old ? Path.Combine(_testProjectFolders.PathToSolutionDirectory, "packages") : _folders.GlobalPackages;

            if (ProjectType == ProjectType.Library)
            {
                _testProjectFolders.ProjectFolder = Path.Combine(_testProjectFolders.PathToSolutionDirectory, _project.Name);
                _testProjectFolders.ProjectBinOutputPath = Path.Combine(_testProjectFolders.ProjectFolder, GetProjectCompilePath(_project));

                _testProjectFolders.TestAssemblyFileName = $"{_project.Name}.dll";
                _testProjectFolders.CompiledAssemblyPath = Path.Combine(_testProjectFolders.ProjectBinOutputPath, _testProjectFolders.TestAssemblyFileName);


                _project.AddNuGetPackage("Microsoft.NET.Test.Sdk", "16.4.0");

                if (_project.ProjectFormat == ProjectFormat.Old)
                {
                    _project.AddNuGetPackage("Cucumber.Messages", "6.0.1", new NuGetPackageAssembly("Cucumber.Messages, Version=6.0.1.0, Culture=neutral, PublicKeyToken=b10c5988214f940c", "net45\\Cucumber.Messages.dll"));
                    _project.AddNuGetPackage("Google.Protobuf", "3.7.0", new NuGetPackageAssembly("Google.Protobuf, Version=3.7.0.0, Culture=neutral, PublicKeyToken=a7d26565bac4d604", "net45\\Google.Protobuf.dll"));
                }

                if (_currentVersionDriver.SpecFlowVersion >= new Version(3, 0))
                {
                    // TODO: dei replace this hack with better logic when SpecFlow 3 can be strong name signed
                    _project.AddNuGetPackage("SpecFlow", _currentVersionDriver.SpecFlowNuGetVersion, new NuGetPackageAssembly("TechTalk.SpecFlow", "net461\\TechTalk.SpecFlow.dll"));
                }
                else
                {
                    _project.AddNuGetPackage("SpecFlow", $"{_currentVersionDriver.SpecFlowNuGetVersion}", new NuGetPackageAssembly($"TechTalk.SpecFlow, Version={_currentVersionDriver.SpecFlowVersion}.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL", "net461\\TechTalk.SpecFlow.dll"));
                }

                _project.AddNuGetPackage("BoDi", "1.4.1", new NuGetPackageAssembly("BoDi, Version=1.4.1.0, Culture=neutral, PublicKeyToken=ff7cd5ea2744b496", "net45\\BoDi.dll"));
                _project.AddNuGetPackage("Gherkin", "6.0.0", new NuGetPackageAssembly("Gherkin, Version=6.0.0.0, Culture=neutral, PublicKeyToken=86496cfa5b4a5851", "net45\\Gherkin.dll"));
                _project.AddNuGetPackage("Utf8Json", "1.3.7", new NuGetPackageAssembly("Utf8Json, Version=1.3.7.0, Culture=neutral, PublicKeyToken=8a73d3ba7e392e27", "net45\\Utf8Json.dll"));
                _project.AddNuGetPackage("System.Threading.Tasks.Extensions", "4.5.1",
                    new NuGetPackageAssembly("System.Threading.Tasks.Extensions, Version=4.2.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "portable-net45+win8+wp8+wpa81\\System.Threading.Tasks.Extensions.dll"));

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

                    if (_project.ProjectFormat == ProjectFormat.Old && _currentVersionDriver.SpecFlowVersion < new Version(3, 0, 0))
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
                    case UnitTestProvider.xUnit:
                        ConfigureXUnit();
                        break;
                    case UnitTestProvider.NUnit3:
                        ConfigureNUnit();
                        break;
                    default:
                        throw new InvalidOperationException(@"Invalid unit test provider.");
                }

                if (_currentVersionDriver.SpecFlowVersion < new Version(3, 0))
                {
                    _project.AddNuGetPackage("Newtonsoft.Json", "11.0.2");
                }

            }

            _project.AddNuGetPackage("FluentAssertions", "5.3.0");
            AddAdditionalStuff();
        }

        private void ConfigureNUnit()
        {
            _project.AddNuGetPackage(NUnit3PackageName, NUnit3PackageVersion);
            _project.AddNuGetPackage(NUnit3TestAdapterPackageName, NUnit3TestAdapterPackageVersion);


            if (_currentVersionDriver.SpecFlowVersion >= new Version(3, 0) && IsSpecFlowFeatureProject)
            {
                _project.AddNuGetPackage("SpecFlow.NUnit", _currentVersionDriver.SpecFlowNuGetVersion,
                    new NuGetPackageAssembly(GetSpecFlowPublicAssemblyName("TechTalk.SpecFlow.NUnit.SpecFlowPlugin.dll"), "net461\\TechTalk.SpecFlow.NUnit.SpecFlowPlugin.dll"));
                Configuration.Plugins.Add(new SpecFlowPlugin("TechTalk.SpecFlow.NUnit", SpecFlowPluginType.Runtime));
            }
        }

        private void ConfigureXUnit()
        {
            if (_project.ProjectFormat == ProjectFormat.New)
            {
                _project.AddNuGetPackage("xunit", XUnitPackageVersion);
            }
            else
            {
                _project.AddNuGetPackage("xunit.core", XUnitPackageVersion);
                _project.AddNuGetPackage("xunit.extensibility.core", XUnitPackageVersion,
                    new NuGetPackageAssembly("xunit.core, Version=2.4.1.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "net452\\xunit.core.dll"));
                _project.AddNuGetPackage("xunit.extensibility.execution", XUnitPackageVersion,
                    new NuGetPackageAssembly("xunit.execution.desktop, Version=2.4.1.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "net452\\xunit.execution.desktop.dll"));
                _project.AddNuGetPackage("xunit.assert", XUnitPackageVersion,
                    new NuGetPackageAssembly("xunit.assert, Version=2.4.1.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.1\\xunit.assert.dll"));
                _project.AddNuGetPackage("xunit.abstractions", "2.0.3",
                    new NuGetPackageAssembly("xunit.abstractions, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.0\\xunit.abstractions.dll"));
            }

            _project.AddNuGetPackage("xunit.runner.visualstudio", XUnitPackageVersion);
            _project.AddNuGetPackage("Xunit.SkippableFact", "1.3.12", new NuGetPackageAssembly("Xunit.SkippableFact, Version=1.3.0.0, Culture=neutral, PublicKeyToken=b2b52da82b58eb73", "net452\\Xunit.SkippableFact.dll"));

            if (_project.ProjectFormat == ProjectFormat.Old)
            {
                _project.AddNuGetPackage("Validation", "2.4.18", new NuGetPackageAssembly("Validation, Version=2.4.0.0, Culture=neutral, PublicKeyToken=2fc06f0d701809a7", "net45\\Validation.dll"));
            }

            if (_currentVersionDriver.SpecFlowVersion >= new Version(3, 0) && IsSpecFlowFeatureProject)
            {
                _project.AddNuGetPackage("SpecFlow.xUnit", _currentVersionDriver.SpecFlowNuGetVersion,
                    new NuGetPackageAssembly(GetSpecFlowPublicAssemblyName("TechTalk.SpecFlow.xUnit.SpecFlowPlugin.dll"), "net461\\TechTalk.SpecFlow.xUnit.SpecFlowPlugin.dll"));
                Configuration.Plugins.Add(new SpecFlowPlugin("TechTalk.SpecFlow.xUnit", SpecFlowPluginType.Runtime));
            }
        }

        private void ConfigureMSTest()
        {
            _project.AddNuGetPackage("MSTest.TestAdapter", MSTestPackageVersion);
            _project.AddNuGetPackage(
                "MSTest.TestFramework",
                MSTestPackageVersion,
                new NuGetPackageAssembly(
                    "Microsoft.VisualStudio.TestPlatform.TestFramework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL",
                    "net45\\Microsoft.VisualStudio.TestPlatform.TestFramework.dll"),
                new NuGetPackageAssembly(
                    "Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL",
                    "net45\\Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll"));

            if (_currentVersionDriver.SpecFlowVersion >= new Version(3, 0) && IsSpecFlowFeatureProject)
            {
                _project.AddNuGetPackage("SpecFlow.MSTest", _currentVersionDriver.SpecFlowNuGetVersion,
                    new NuGetPackageAssembly(GetSpecFlowPublicAssemblyName("TechTalk.SpecFlow.MSTest.SpecFlowPlugin.dll"), "net461\\TechTalk.SpecFlow.MSTest.SpecFlowPlugin.dll"));
                Configuration.Plugins.Add(new SpecFlowPlugin("TechTalk.SpecFlow.MSTest", SpecFlowPluginType.Runtime));
            }
        }

        private void ConfigureRunner()
        {
            _project.AddNuGetPackage("SpecRun.Runner", _currentVersionDriver.NuGetVersion);

            if (_currentVersionDriver.SpecFlowVersion >= new Version(3, 0))
            {
                if (IsSpecFlowFeatureProject)
                {
                    ConfigureRunnerForSpecFlow3();
                }
            }
            else
            {
                ConfigureRunnerForSpecFlow2(); 
            }
                
        }

        private void ConfigureRunnerForSpecFlow2()
        {
            _project.AddNuGetPackage($"SpecRun.SpecFlow.{_currentVersionDriver.SpecFlowVersionDash}", _currentVersionDriver.NuGetVersion,
                                     new NuGetPackageAssembly($"SpecRun.SpecFlowPlugin, Version={_currentVersionDriver.MajorMinorPatchVersion}, Culture=neutral, processorArchitecture=MSIL", "net45\\SpecRun.SpecFlowPlugin.dll"),
                                     new NuGetPackageAssembly($"TechTalk.SpecRun, Version={_currentVersionDriver.MajorMinorPatchVersion}, Culture=neutral, PublicKeyToken=d0fc5cc18b3b389b, processorArchitecture=MSIL",
                                                              "net45\\TechTalk.SpecRun.dll"),
                                     new NuGetPackageAssembly($"TechTalk.SpecRun.Common, Version={_currentVersionDriver.MajorMinorPatchVersion}, Culture=neutral, PublicKeyToken=d0fc5cc18b3b389b, processorArchitecture=MSIL",
                                                              "net45\\TechTalk.SpecRun.Common.dll")
            );

            Configuration.Plugins.Add(new SpecFlowPlugin("SpecRun"));
        }

        private void ConfigureRunnerForSpecFlow3()
        {
            _project.AddNuGetPackage($"SpecRun.SpecFlow.{_currentVersionDriver.SpecFlowVersionDash}", _currentVersionDriver.NuGetVersion,
                new NuGetPackageAssembly($"SpecRun.Runtime.SpecFlowPlugin, Version={_currentVersionDriver.MajorMinorPatchVersion}, Culture=neutral, processorArchitecture=MSIL",
                    $"{TargetFrameworkMoniker}\\SpecRun.Runtime.SpecFlowPlugin.dll"),
                new NuGetPackageAssembly($"TechTalk.SpecRun, Version={_currentVersionDriver.MajorMinorPatchVersion}, Culture=neutral, PublicKeyToken=d0fc5cc18b3b389b, processorArchitecture=MSIL",
                    $"{TargetFrameworkMoniker}\\TechTalk.SpecRun.dll"),
                new NuGetPackageAssembly($"TechTalk.SpecRun.Common, Version={_currentVersionDriver.MajorMinorPatchVersion}, Culture=neutral, PublicKeyToken=d0fc5cc18b3b389b, processorArchitecture=MSIL",
                    $"{TargetFrameworkMoniker}\\TechTalk.SpecRun.Common.dll")
            );
        }

        protected virtual void AddAdditionalStuff()
        {
        }

        private void AddUnitTestProviderSpecificConfig()
        {
            switch (Configuration.UnitTestProvider)
            {
                case UnitTestProvider.xUnit when !_parallelTestExecution:
                    _project.AddFile(new ProjectFile("XUnitConfiguration.cs", "Compile", "using Xunit; [assembly: CollectionBehavior(MaxParallelThreads = 1, DisableTestParallelization = true)]"));
                    break;
                case UnitTestProvider.xUnit:
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
            return $"{Path.GetFileNameWithoutExtension(assemblyName)}, Version={_currentVersionDriver.SpecFlowVersion}, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL";
#else
            return Path.GetFileNameWithoutExtension(assemblyName);
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
                return Path.Combine("bin", "Debug", _targetFrameworkMonikerStringBuilder.BuildTargetFrameworkMoniker(project.TargetFrameworks).Split(';')[0]);
            }

            return Path.Combine("bin", "Debug");
        }

        public void AddMSBuildTarget(string targetName, string implementation)
        {
            _project.AddTarget(targetName, implementation);
        }

        public void AddNuGetPackage(string nugetPackage, string nugetVersion = null)
        {
            EnsureProjectExists();
            _project.AddNuGetPackage(nugetPackage, nugetVersion);
        }
    }
}
