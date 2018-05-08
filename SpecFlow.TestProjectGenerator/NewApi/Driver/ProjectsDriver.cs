using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        private readonly HooksDriver _hooksDriver;
        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly ConfigurationGeneratorFactory _configurationGeneratorFactory;
        private readonly CurrentVersionDriver _currentVersionDriver;
        private readonly Dictionary<string, ProjectBuilder> _projects;

        public ProjectsDriver(HooksDriver hooksDriver, FeatureFileGenerator featureFileGenerator, BindingsGeneratorFactory bindingsGeneratorFactory, ConfigurationGeneratorFactory configurationGeneratorFactory, CurrentVersionDriver currentVersionDriver)
        {
            _hooksDriver = hooksDriver;
            _featureFileGenerator = featureFileGenerator;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _configurationGeneratorFactory = configurationGeneratorFactory;
            _currentVersionDriver = currentVersionDriver;

            Projects = _projects = new Dictionary<string, ProjectBuilder>();
            DefaultProject = CreateProjectInternal(DefaultProjectName, ProgrammingLanguage.CSharp);
        }

        public IReadOnlyDictionary<string, ProjectBuilder> Projects { get; }

        public ProjectBuilder DefaultProject { get; }

        public string CreateProject(string language)
        {
            return CreateProjectInternal(null, ParseProgrammingLanguage(language)).ProjectName;
        }

        public string CreateProject(ProgrammingLanguage language)
        {
            return CreateProjectInternal(null, language).ProjectName;
        }

        public void CreateProject(string projectName, string language)
        {
            CreateProjectInternal(projectName, ParseProgrammingLanguage(language));
        }

        public void CreateProject(string projectName, ProgrammingLanguage language)
        {
            CreateProjectInternal(projectName, language);
        }

        public void AddHookBinding(string eventType, string name, string tags, string code = "", int? order = null, bool useScopeTagsOnHookMethods = false, bool useScopeTagsOnClass = false)
        {
            IEnumerable<string> ToTagsList(string input) => input.Split(',');

            AddHookBinding(DefaultProject, eventType, name, code, order, ToTagsList(tags), useScopeTagsOnHookMethods, useScopeTagsOnClass);
        }

        public void AddHookBinding(string eventType, string name, string code = "", int? order = null, IEnumerable<string> tags = null, bool useScopeTagsOnHookMethods = false, bool useScopeTagsOnClass = false)
        {
            AddHookBinding(DefaultProject, eventType, name, code, order, tags, useScopeTagsOnHookMethods, useScopeTagsOnClass);
        }

        public void AddHookBinding(string projectName, string eventType, string name, string code = "", int? order = null, IEnumerable<string> tags = null, bool useScopeTagsOnHookMethods = false, bool useScopeTagsOnClass = false)
        {
            AddHookBinding(Projects[projectName], eventType, name, code, order, tags, useScopeTagsOnHookMethods, useScopeTagsOnClass);
        }

        public void AddHookBinding(ProjectBuilder project, string eventType, string name, string code = "", int? order = null, IEnumerable<string> tags = null, bool useScopeTagsOnHookMethods = false, bool useScopeTagsOnClass = false)
        {
            project.AddHookBinding(eventType, name, code, order, tags, useScopeTagsOnHookMethods, useScopeTagsOnClass);
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

        private ProjectBuilder CreateProjectInternal(string projectName, ProgrammingLanguage language)
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
                case "VB.NET":
                case "VBNET": return ProgrammingLanguage.VB;
                case "FSHARP":
                case "F#": return ProgrammingLanguage.FSharp;
                default: return ProgrammingLanguage.Other;
            }
        }

        private bool IsStaticEvent(string eventType)
        {
            return
                eventType == "BeforeFeature" ||
                eventType == "AfterFeature" ||
                eventType == "BeforeTestRun" ||
                eventType == "AfterTestRun";
        }
    }
}
