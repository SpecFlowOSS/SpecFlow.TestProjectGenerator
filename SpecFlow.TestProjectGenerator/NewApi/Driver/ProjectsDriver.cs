using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private readonly TestProjectFolders _testProjectFolders;
        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly ConfigurationGeneratorFactory _configurationGeneratorFactory;
        private readonly CurrentVersionDriver _currentVersionDriver;
        private readonly TestRunConfiguration _testRunConfiguration;
        private readonly Dictionary<string, ProjectBuilder> _projects;

        public ProjectsDriver(TestProjectFolders testProjectFolders, FeatureFileGenerator featureFileGenerator, BindingsGeneratorFactory bindingsGeneratorFactory, ConfigurationGeneratorFactory configurationGeneratorFactory, CurrentVersionDriver currentVersionDriver, TestRunConfiguration testRunConfiguration)
        {
            _testProjectFolders = testProjectFolders;
            _featureFileGenerator = featureFileGenerator;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _configurationGeneratorFactory = configurationGeneratorFactory;
            _currentVersionDriver = currentVersionDriver;
            _testRunConfiguration = testRunConfiguration;

            Projects = _projects = new Dictionary<string, ProjectBuilder>();
        }
         
        public IReadOnlyDictionary<string, ProjectBuilder> Projects { get; }

        private ProjectBuilder _defaultProject;
        public ProjectBuilder DefaultProject
        {
            get
            {
                if (_defaultProject == null)
                {
                    _defaultProject = CreateProjectInternal(DefaultProjectName, _testRunConfiguration.ProgrammingLanguage);
                }
                return _defaultProject;
            }

        }

        public string CreateProject(string language)
        {
            var projectBuilder = CreateProjectInternal(null, ParseProgrammingLanguage(language));

            if (_defaultProject == null)
            {
                _defaultProject = projectBuilder;
            }

            return projectBuilder.ProjectName;
        }

        public void CreateProject(string projectName, string language)
        {
            var projectBuilder = CreateProjectInternal(projectName, ParseProgrammingLanguage(language));
            if (_defaultProject == null)
            {
                _defaultProject = projectBuilder;
            }
        }

        public void AddHookBinding(string eventType, string name, string hookTypeAttributeTagsString, string methodScopeAttributeTagsString = null, string classScopeAttributeTagsString = null, string code = "", int? order = null)
        {
            var hookTypeAttributeTags = hookTypeAttributeTagsString?.Split(',').Select(t => t.Trim()).ToArray();
            var methodScopeAttributeTags = methodScopeAttributeTagsString?.Split(',').Select(t => t.Trim()).ToArray();
            var classScopeAttributeTags = classScopeAttributeTagsString?.Split(',').Select(t => t.Trim()).ToArray();

            AddHookBinding(DefaultProject, eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        public void AddHookBinding(string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null, IList<string> classScopeAttributeTags = null)
        {
            AddHookBinding(DefaultProject, eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        public void AddHookBinding(string projectName, string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null, IList<string> classScopeAttributeTags = null)
        {
            AddHookBinding(Projects[projectName], eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        private void AddHookBinding(ProjectBuilder project, string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null,  IList<string> classScopeAttributeTags = null)
        {
            if (code is null)
            {
                code = $"System.IO.File.AppendAllText(System.IO.Path.Combine({_testProjectFolders.PathToSolutionDirectory}, \"hooks.log\"), \"-> hook: {name}\");";
            }

            project.AddHookBinding(eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
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
                Language = language,
                Format = _testRunConfiguration.ProjectFormat,
                TargetFrameworks = _testRunConfiguration.TargetFramework
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

        public void AddFile(string fileName, string fileContent, string compileAction = "None")
        {
            DefaultProject.AddFile(new ProjectFile(fileName, compileAction, fileContent, CopyToOutputDirectory.CopyAlways));
        }

        public void EnableTestParallelExecution()
        {
            DefaultProject.EnableParallelTestExecution();
        }
    }
}
