using System.Collections.Generic;
using System.IO;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class ProjectsDriver
    {
        public const string DefaultProjectName = "DefaultTestProject";

        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly AppConfigGenerator _appConfigGenerator;
        private readonly CurrentVersionDriver _currentVersionDriver;
        private readonly Dictionary<string, ProjectBuilder> _projects;

        public ProjectsDriver(FeatureFileGenerator featureFileGenerator, BindingsGeneratorFactory bindingsGeneratorFactory, AppConfigGenerator appConfigGenerator, CurrentVersionDriver currentVersionDriver)
        {
            _featureFileGenerator = featureFileGenerator;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _appConfigGenerator = appConfigGenerator;
            _currentVersionDriver = currentVersionDriver;

            DefaultProject = new ProjectBuilder(_featureFileGenerator, _bindingsGeneratorFactory, _appConfigGenerator, new Configuration(), _currentVersionDriver)
            {
                ProjectName = DefaultProjectName
            };

            Projects = _projects = new Dictionary<string, ProjectBuilder>
            {
                [DefaultProjectName] = DefaultProject
            };
        }

        public IReadOnlyDictionary<string, ProjectBuilder> Projects { get; }
        public ProjectBuilder DefaultProject { get; }

        public string CreateProject(string language)
        {
            var project = new ProjectBuilder(_featureFileGenerator, _bindingsGeneratorFactory, _appConfigGenerator, new Configuration(), _currentVersionDriver)
            {
                Language = ParseProgrammingLanguage(language)
            };

            _projects.Add(project.ProjectName, project);
            return project.ProjectName;
        }

        public void CreateProject(string projectName, string language)
        {
            var project = new ProjectBuilder(_featureFileGenerator, _bindingsGeneratorFactory, _appConfigGenerator, new Configuration(), _currentVersionDriver)
            {
                ProjectName = projectName,
                Language = ParseProgrammingLanguage(language)
            };
            _projects.Add(projectName, project);
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

        public void AddBindingCode(string projectName, string bindingCode)
        {
            var targetProject = Projects[projectName];
            AddBindingCode(targetProject, bindingCode);
        }

        public void AddBindingCode(string bindingCode)
        {
            AddBindingCode(DefaultProject, bindingCode);
        }

        public void AddProjectReference(string projectName, string projectNameToReference)
        {
            var targetProject = Projects[projectName];
            AddProjectReference(targetProject, projectNameToReference);
        }

        public void AddProjectReference(string projectNameToReference)
        {
            AddProjectReference(DefaultProject, projectNameToReference);
        }

        private void AddBindingCode(ProjectBuilder targetProject, string bindingCode)
        {
            targetProject.AddBindingCode(bindingCode);
        }

        private void AddProjectReference(ProjectBuilder targetProject, string projectNameToReference)
        {
            var projectToReference = Projects[projectNameToReference];
            targetProject.AddProjectReference(Path.Combine(@"..\", projectNameToReference, $"{projectNameToReference}.{projectToReference.Language.ToProjectFileExtension()}"), projectToReference);
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
