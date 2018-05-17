using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Helpers;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class ProjectsDriver
    {
        private readonly SolutionDriver _solutionDriver;
        private readonly ProjectBuilderFactory _projectBuilderFactory;
        private readonly TestProjectFolders _testProjectFolders;

        public ProjectsDriver(SolutionDriver solutionDriver, ProjectBuilderFactory projectBuilderFactory, TestProjectFolders testProjectFolders)
        {
            _solutionDriver = solutionDriver;
            _projectBuilderFactory = projectBuilderFactory;
            _testProjectFolders = testProjectFolders;
        }

        public string CreateProject(string language)
        {
            var projectBuilder = _projectBuilderFactory.CreateProject(language);
            _solutionDriver.AddProject(projectBuilder);
            return projectBuilder.ProjectName;
        }

        public void CreateProject(string projectName, string language)
        {
            var projectBuilder = _projectBuilderFactory.CreateProject(projectName, language);
            _solutionDriver.AddProject(projectBuilder);
        }

        public void AddHookBinding(string eventType, string name, string hookTypeAttributeTagsString, string methodScopeAttributeTagsString = null, string classScopeAttributeTagsString = null, string code = "", int? order = null)
        {
            var hookTypeAttributeTags = hookTypeAttributeTagsString?.Split(',').Select(t => t.Trim()).ToArray();
            var methodScopeAttributeTags = methodScopeAttributeTagsString?.Split(',').Select(t => t.Trim()).ToArray();
            var classScopeAttributeTags = classScopeAttributeTagsString?.Split(',').Select(t => t.Trim()).ToArray();

            AddHookBinding(_solutionDriver.DefaultProject, eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        public void AddHookBinding(string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null, IList<string> classScopeAttributeTags = null)
        {
            AddHookBinding(_solutionDriver.DefaultProject, eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        private void AddHookBinding(ProjectBuilder project, string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null,  IList<string> classScopeAttributeTags = null)
        {
            code = $"System.IO.File.AppendAllText(System.IO.Path.Combine(@\"{_testProjectFolders.PathToSolutionDirectory}\", \"hooks.log\"), \"-> hook: {name}\");{Environment.NewLine}{code}";
            project.AddHookBinding(eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        public void AddFeatureFile(string featureFileContent)
        {
            _solutionDriver.DefaultProject.AddFeatureFile(featureFileContent);
        }

        public void AddStepBinding(string attributeName, string regex, string csharpcode, string vbnetcode)
        {
            _solutionDriver.DefaultProject.AddStepBinding(attributeName, regex, csharpcode, vbnetcode);
        }

        public void AddLoggingStepBinding(string attributeName, string methodName, string regex)
        {
            _solutionDriver.DefaultProject.AddLoggingStepBinding(attributeName, methodName, Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log"), regex);
        }

        public void AddStepBinding(string projectName, string bindingCode) => AddStepBinding(_solutionDriver.Projects[projectName], bindingCode);

        public void AddStepBinding(string bindingCode) => AddStepBinding(_solutionDriver.DefaultProject, bindingCode);

        public void AddProjectReference(string projectNameToReference)
        {
            AddProjectReference(_solutionDriver.DefaultProject, projectNameToReference);
        }

        public void AddBindingClass(string rawBindingClass) => AddBindingClass(_solutionDriver.DefaultProject, rawBindingClass);

        private void AddStepBinding(ProjectBuilder targetProject, string bindingCode)
        {
            targetProject.AddStepBinding(bindingCode);
        }

        private void AddProjectReference(ProjectBuilder targetProject, string projectNameToReference)
        {
            var projectToReference = _solutionDriver.Projects[projectNameToReference];
            targetProject.AddProjectReference(Path.Combine(@"..\", projectNameToReference, $"{projectNameToReference}.{projectToReference.Language.ToProjectFileExtension()}"), projectToReference);
        }

        private void AddBindingClass(ProjectBuilder project, string rawBindingClass)
        {
            project.AddBindingClass(rawBindingClass);
        }

        public void AddFile(string fileName, string fileContent, string compileAction = "None")
        {
            _solutionDriver.DefaultProject.AddFile(new ProjectFile(fileName, compileAction, fileContent, CopyToOutputDirectory.CopyAlways));
        }

        public void EnableTestParallelExecution()
        {
            _solutionDriver.DefaultProject.EnableParallelTestExecution();
        }
    }
}
