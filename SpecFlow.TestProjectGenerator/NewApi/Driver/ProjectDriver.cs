using System;
using System.Collections.Generic;
using System.Text;
using SpecFlow.TestProjectGenerator.Inputs;
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
        private Project _project;
        private ProgrammingLanguage _programmingLanguage = ProgrammingLanguage.CSharp;
        private TargetFramework _targetFrameworks = TargetFramework.Net45;
        private ProjectFormat _projectFormat = ProjectFormat.Old;

        public ProjectDriver(FeatureFileGenerator featureFileGenerator, SolutionDriver solutionDriver, BindingsGeneratorFactory bindingsGeneratorFactory, AppConfigGenerator appConfigGenerator)
        {
            _featureFileGenerator = featureFileGenerator;
            _solutionDriver = solutionDriver;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _appConfigGenerator = appConfigGenerator;
        }

        public Guid ProjectGuid { get; } = Guid.NewGuid();
        public string ProjectName => $"TestProject_{ProjectGuid:N}";


        private void EnsureProjectExists()
        {
            if (_project != null)
            {
                return;
            }

            _project = new Project(ProjectName, _programmingLanguage, _targetFrameworks, _projectFormat);
            _project.AddNuGetPackage("SpecFlow", "1.0.0-alpha", new NuGetPackageAssembly("TechTalk.SpecFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL", "net45\\TechTalk.SpecFlow.dll")); //TODO change after GitVersion adding
            _project.AddFile(_appConfigGenerator.Generate("xunit"));
            
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
    }

    public enum ParameterType
    {
        Normal,
        Table,
        DocString,
    }
}
