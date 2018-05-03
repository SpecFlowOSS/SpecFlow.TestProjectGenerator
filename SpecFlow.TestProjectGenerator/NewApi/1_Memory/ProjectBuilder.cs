using System;
using SpecFlow.TestProjectGenerator.NewApi.Driver;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationModel;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectBuilder
    {
        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly ConfigurationGeneratorFactory _configurationGeneratorFactory;

        private readonly CurrentVersionDriver _currentVersionDriver;

        private Project _project;

        public ProjectBuilder(FeatureFileGenerator featureFileGenerator, BindingsGeneratorFactory bindingsGeneratorFactory, ConfigurationGeneratorFactory configurationGeneratorFactory, Configuration configuration, CurrentVersionDriver currentVersionDriver)
        {
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

        public void AddStepBinding(string scenarioBlock, string regex, string csharpcode, string vbnetcode)
        {
            EnsureProjectExists();

            var methodImplementation = GetCode(_project.ProgrammingLanguage, csharpcode, vbnetcode);
            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);

            _project.AddFile(bindingsGenerator.GenerateStepDefinition("StepBinding", methodImplementation, scenarioBlock, regex));
            _project.AddFile(bindingsGenerator.GenerateStepDefinition("StepBinding", methodImplementation, scenarioBlock, regex, ParameterType.Table, "tableArg"));
            _project.AddFile(bindingsGenerator.GenerateStepDefinition("StepBinding", methodImplementation, scenarioBlock, regex, ParameterType.DocString, "docStringArg"));
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
            _project.AddNuGetPackage("FSharp.Core", "4.3.4");
        }

        private void EnsureProjectExists()
        {
            if (_project != null)
            {
                return;
            }

            _project = new Project(ProjectName, ProjectGuid, Language, TargetFrameworks, Format);
            _project.AddNuGetPackage("BoDi", "1.4.0-alpha", new NuGetPackageAssembly("BoDi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ff7cd5ea2744b496", "net45\\BoDi.dll"));
#if SPECFLOW_ENABLE_STRONG_NAME_SIGNING
            _project.AddNuGetPackage("SpecFlow", _currentVersionDriver.GitVersionInfo.NuGetVersion, new NuGetPackageAssembly($"TechTalk.SpecFlow, Version={_currentVersionDriver.GitVersionInfo.MajorMinorPatch}.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL", "net45\\TechTalk.SpecFlow.dll"));
#else
            _project.AddNuGetPackage("SpecFlow", _currentVersionDriver.GitVersionInfo.NuGetVersion, new NuGetPackageAssembly("TechTalk.SpecFlow", "net45\\TechTalk.SpecFlow.dll"));
#endif
            
            if (_project.ProgrammingLanguage == ProgrammingLanguage.FSharp)
            {
                AddInitialFSharpReferences();
            }

            if (IsSpecFlowFeatureProject)
            {
                _project.AddNuGetPackage("SpecFlow.Tools.MsBuild.Generation", _currentVersionDriver.GitVersionInfo.NuGetVersion);
            }

            switch (Configuration.UnitTestProvider)
            {
                case UnitTestProvider.SpecRun:
                    throw new NotImplementedException();
                    break;
                case UnitTestProvider.SpecRunWithNUnit:
                    throw new NotImplementedException();
                    break;
                case UnitTestProvider.SpecRunWithNUnit2:
                    throw new NotImplementedException();
                    break;
                case UnitTestProvider.SpecRunWithMsTest:
                    throw new NotImplementedException();
                    break;
                case UnitTestProvider.MSTest:
                    throw new NotImplementedException();
                    break;
                case UnitTestProvider.XUnit:
                    _project.AddNuGetPackage("xunit.core", "2.3.1");
                    _project.AddNuGetPackage("xunit.extensibility.core", "2.3.1", new NuGetPackageAssembly("xunit.core, Version=2.3.1.3858, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.1\\xunit.core.dll"));
                    _project.AddNuGetPackage("xunit.extensibility.execution", "2.3.1", new NuGetPackageAssembly("xunit.execution.desktop, Version=2.3.1.3858, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "net452\\xunit.execution.desktop.dll"));
                    _project.AddNuGetPackage("xunit.assert", "2.3.1", new NuGetPackageAssembly("xunit.assert, Version=2.3.1.3858, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.1\\xunit.assert.dll"));
                    _project.AddNuGetPackage("xunit.abstractions", "2.0.1", new NuGetPackageAssembly("xunit.abstractions, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.0\\xunit.abstractions.dll"));
                    _project.AddNuGetPackage("xunit.runner.visualstudio", "2.3.1");
                    break;
                case UnitTestProvider.NUnit3:
                    throw new NotImplementedException();
                    break;
                case UnitTestProvider.NUnit2:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _project.AddNuGetPackage("FluentAssertions", "5.3.0", new NuGetPackageAssembly("FluentAssertions, Version=5.3.0.0, Culture=neutral, PublicKeyToken=33f2691a05b67b6a", @"net45\FluentAssertions.dll"));
        }
    }
}
