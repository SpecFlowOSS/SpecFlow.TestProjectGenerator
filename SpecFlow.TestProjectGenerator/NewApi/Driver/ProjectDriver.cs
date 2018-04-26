using System;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class ProjectDriver
    {
        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly SolutionDriver _solutionDriver;
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly AppConfigGenerator _appConfigGenerator;
        private readonly Configuration _configuration;
        private readonly CurrentVersionDriver _currentVersionDriver;
        private Project _project;
        private ProgrammingLanguage _programmingLanguage = ProgrammingLanguage.CSharp;
        private TargetFramework _targetFrameworks = TargetFramework.Net452;
        private ProjectFormat _projectFormat = ProjectFormat.Old;

        public ProjectDriver(FeatureFileGenerator featureFileGenerator, SolutionDriver solutionDriver, BindingsGeneratorFactory bindingsGeneratorFactory, AppConfigGenerator appConfigGenerator, Configuration configuration, CurrentVersionDriver currentVersionDriver)
        {
            _currentVersionDriver = currentVersionDriver;
            _featureFileGenerator = featureFileGenerator;
            _solutionDriver = solutionDriver;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _appConfigGenerator = appConfigGenerator;
            _configuration = configuration;
        }

        public Guid ProjectGuid { get; } = Guid.NewGuid();
        public string ProjectName => $"TestProject_{ProjectGuid:N}";

        private void EnsureProjectExists()
        {
            if (_project != null)
            {
                return;
            }

            _project = new Project(ProjectName, ProjectGuid,  _programmingLanguage, _targetFrameworks, _projectFormat);
            _project.AddNuGetPackage("BoDi", "1.4.0-alpha", new NuGetPackageAssembly("BoDi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ff7cd5ea2744b496", "net45\\BoDi.dll"));
#if SPECFLOW_ENABLE_STRONG_NAME_SIGNING
            _project.AddNuGetPackage("SpecFlow", _currentVersionDriver.GitVersionInfo.NuGetVersion, new NuGetPackageAssembly($"TechTalk.SpecFlow, Version={_currentVersionDriver.GitVersionInfo.MajorMinorPatch}.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL", "net45\\TechTalk.SpecFlow.dll"));
#else
            _project.AddNuGetPackage("SpecFlow", _currentVersionDriver.GitVersionInfo.NuGetVersion, new NuGetPackageAssembly("TechTalk.SpecFlow", "net45\\TechTalk.SpecFlow.dll"));
#endif
            _project.AddNuGetPackage("SpecFlow.Tools.MsBuild.Generation", _currentVersionDriver.GitVersionInfo.NuGetVersion);


            switch (_configuration.UnitTestProvider)
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

            
            _project.AddFile(_appConfigGenerator.Generate(_configuration));
            
            _solutionDriver.AddProject(_project);
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

        public void AddBindingCode(string bindingCode)
        {
            EnsureProjectExists();

            
            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);
            _project.AddFile(bindingsGenerator.GenerateStepDefinition(bindingCode));
        }
    }

    public enum ParameterType
    {
        Normal,
        Table,
        DocString,
    }
}
