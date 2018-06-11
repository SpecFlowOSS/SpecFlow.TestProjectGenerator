using System;
using System.Collections.Generic;
using System.IO;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi.Driver;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationModel;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectBuilder
    {
        private readonly TestProjectFolders _testProjectFolders;
        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly ConfigurationGeneratorFactory _configurationGeneratorFactory;
        private readonly CurrentVersionDriver _currentVersionDriver;
        private Project _project;
        private bool _parallelTestExecution;

        public ProjectBuilder(TestProjectFolders testProjectFolders, FeatureFileGenerator featureFileGenerator, BindingsGeneratorFactory bindingsGeneratorFactory, ConfigurationGeneratorFactory configurationGeneratorFactory, Configuration configuration, CurrentVersionDriver currentVersionDriver)
        {
            _testProjectFolders = testProjectFolders;
            _featureFileGenerator = featureFileGenerator;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _configurationGeneratorFactory = configurationGeneratorFactory;
            Configuration = configuration;
            _currentVersionDriver = currentVersionDriver;
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
            _project.AddFile(bindingsGenerator.GenerateBindingClassFile(fullBindingClass));
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
            _project.AddNuGetPackage("BoDi", "1.4.0-alpha1", new NuGetPackageAssembly("BoDi, Version=1.4.0.0, Culture=neutral, PublicKeyToken=ff7cd5ea2744b496", "net45\\BoDi.dll"));
            _project.AddNuGetPackage("SpecFlow", _currentVersionDriver.NuGetVersion, new NuGetPackageAssembly(GetSpecFlowPublicAssemblyName("TechTalk.SpecFlow"), "net45\\TechTalk.SpecFlow.dll"));

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
                _project.AddNuGetPackage("SpecFlow.Tools.MsBuild.Generation", _currentVersionDriver.NuGetVersion);
            }

            switch (Configuration.UnitTestProvider)
            {
                case UnitTestProvider.SpecRun:
                    throw new NotImplementedException();
                case UnitTestProvider.SpecRunWithNUnit:
                    throw new NotImplementedException();
                case UnitTestProvider.SpecRunWithNUnit2:
                    throw new NotImplementedException();
                case UnitTestProvider.SpecRunWithMsTest:
                    throw new NotImplementedException();
                case UnitTestProvider.MSTest:
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
                    break;
                case UnitTestProvider.XUnit:
                    _project.AddNuGetPackage("xunit.core", "2.3.1");
                    _project.AddNuGetPackage("xunit.extensibility.core", "2.3.1", new NuGetPackageAssembly("xunit.core, Version=2.3.1.3858, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.1\\xunit.core.dll"));
                    _project.AddNuGetPackage("xunit.extensibility.execution", "2.3.1", new NuGetPackageAssembly("xunit.execution.desktop, Version=2.3.1.3858, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "net452\\xunit.execution.desktop.dll"));
                    _project.AddNuGetPackage("xunit.assert", "2.3.1", new NuGetPackageAssembly("xunit.assert, Version=2.3.1.3858, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.1\\xunit.assert.dll"));
                    _project.AddNuGetPackage("xunit.abstractions", "2.0.1", new NuGetPackageAssembly("xunit.abstractions, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.0\\xunit.abstractions.dll"));
                    _project.AddNuGetPackage("xunit.runner.visualstudio", "2.3.1");
                    _project.AddNuGetPackage("SpecFlow.xUnit", _currentVersionDriver.NuGetVersion, new NuGetPackageAssembly(GetSpecFlowPublicAssemblyName("TechTalk.SpecFlow.xUnit.SpecFlowPlugin.dll"), "net45\\TechTalk.SpecFlow.xUnit.SpecFlowPlugin.dll"));
                    Configuration.Plugins.Add(new SpecFlowPlugin("TechTalk.SpecFlow.xUnit", SpecFlowPluginType.Runtime));
                    break;
                case UnitTestProvider.NUnit3:
                    _project.AddNuGetPackage("NUnit", "3.8.1", new NuGetPackageAssembly("nunit.framework, Version=3.8.1.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL", "net45\\nunit.framework.dll"));
                    _project.AddNuGetPackage("NUnit3TestAdapter", "3.8.0");
                    break;
                case UnitTestProvider.NUnit2:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _project.AddNuGetPackage("Newtonsoft.Json", "11.0.2", new NuGetPackageAssembly("Newtonsoft.Json, Version=11.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL", "net45\\Newtonsoft.Json.dll"));
            _project.AddNuGetPackage("FluentAssertions", "5.3.0", new NuGetPackageAssembly("FluentAssertions, Version=5.3.0.0, Culture=neutral, PublicKeyToken=33f2691a05b67b6a", @"net45\FluentAssertions.dll"));

            AddAdditionalStuff();
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
            return $"{assemblyName}, Version={_currentVersionDriver.SpecFlowVersion}, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL";
#else
            return assemblyName;
#endif
        }

        public void EnableParallelTestExecution()
        {
            _parallelTestExecution = true;
        }
    }
}
