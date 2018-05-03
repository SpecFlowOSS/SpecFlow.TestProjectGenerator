using System.Collections.Generic;
using System.IO;
using SpecFlow.TestProjectGenerator.Helpers;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationModel;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class ProjectsDriver
    {
        public const string DefaultProjectName = "DefaultTestProject";

        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly ConfigurationGeneratorFactory _configurationGeneratorFactory;
        private readonly CurrentVersionDriver _currentVersionDriver;
        private readonly Dictionary<string, ProjectBuilder> _projects;

        public ProjectsDriver(FeatureFileGenerator featureFileGenerator, BindingsGeneratorFactory bindingsGeneratorFactory, ConfigurationGeneratorFactory configurationGeneratorFactory, CurrentVersionDriver currentVersionDriver)
        {
            _featureFileGenerator = featureFileGenerator;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _configurationGeneratorFactory = configurationGeneratorFactory;
            _currentVersionDriver = currentVersionDriver;

            Projects = _projects = new Dictionary<string, ProjectBuilder>();
            DefaultProject = CreateProject(DefaultProjectName, ProgrammingLanguage.CSharp);
        }

        public IReadOnlyDictionary<string, ProjectBuilder> Projects { get; }
        public ProjectBuilder DefaultProject { get; }

        public string CreateProject(string language)
        {
            return CreateProject(null, ParseProgrammingLanguage(language)).ProjectName;
        }

        public void CreateProject(string projectName, string language)
        {
            CreateProject(projectName, ParseProgrammingLanguage(language));
        }

        public void AddFeatureFile(string projectName, string featureFileContent)
        {
            Projects[projectName].AddFeatureFile(featureFileContent);
        }

        public void AddFeatureFile(string featureFileContent)
        {
            DefaultProject.AddFeatureFile(featureFileContent);
        }

        public void AddStepBinding(string projectName, string scenarioBlock, string regex, string csharpcode, string vbnetcode)
        {
           Projects[projectName].AddStepBinding(scenarioBlock, regex, csharpcode, vbnetcode);
        }

        public void AddStepBinding(string scenarioBlock, string regex, string csharpcode, string vbnetcode)
        {
            DefaultProject.AddStepBinding(scenarioBlock, regex, csharpcode, vbnetcode);
        }

        public void AddStepBinding(string projectName, string bindingCode) => AddStepBinding(Projects[projectName], bindingCode);
        public void AddStepBinding(string bindingCode) => AddStepBinding(DefaultProject, bindingCode);

        public void AddProjectReference(string projectName, string projectNameToReference)
        {
            var targetProject = Projects[projectName];
            AddProjectReference(targetProject, projectNameToReference);
        }

        public void AddProjectReference(string projectNameToReference)
        {
            AddProjectReference(DefaultProject, projectNameToReference);
        }

        public void AddBindingClass(string rawBindingClass) => AddBindingClass(DefaultProject, rawBindingClass);

        public void AddBindingClass(string projectName, string rawBindingClass) => AddBindingClass(Projects[projectName], rawBindingClass);

        private ProjectBuilder CreateProject(string projectName, ProgrammingLanguage language)
        {
            var project = new ProjectBuilder(_featureFileGenerator, _bindingsGeneratorFactory, _configurationGeneratorFactory, new Configuration(), _currentVersionDriver)
            {
                Language = language
            };

            if (projectName.IsNotNullOrWhiteSpace())
            {
                project.ProjectName = projectName;
            }

            _projects.Add(project.ProjectName, project);
            return project;
        }

        private void AddStepBinding(ProjectBuilder targetProject, string bindingCode)
        {
            targetProject.AddStepBinding(bindingCode);
        }

        private void AddProjectReference(ProjectBuilder targetProject, string projectNameToReference)
        {
            var projectToReference = Projects[projectNameToReference];
            targetProject.AddProjectReference(Path.Combine(@"..\", projectNameToReference, $"{projectNameToReference}.{projectToReference.Language.ToProjectFileExtension()}"), projectToReference);
        }

        private void AddBindingClass(ProjectBuilder project, string rawBindingClass)
        {
            project.AddBindingClass(rawBindingClass);
        }

        public ProgrammingLanguage ParseProgrammingLanguage(string input)
        {
            switch (input.ToUpper())
            {
                case "CSHARP":
                case "C#": return ProgrammingLanguage.CSharp;
                case "VB":
                case "VBNET": return ProgrammingLanguage.VB;
                case "FSHARP":
                case "F#": return ProgrammingLanguage.FSharp;
                default: return ProgrammingLanguage.Other;
            }
        }
    }
}
