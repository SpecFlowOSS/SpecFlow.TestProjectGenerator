using TechTalk.SpecFlow.TestProjectGenerator.Helpers;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationModel;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectBuilderFactory
    {
        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly ConfigurationGeneratorFactory _configurationGeneratorFactory;
        private readonly CurrentVersionDriver _currentVersionDriver;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly TestRunConfiguration _testRunConfiguration;

        public ProjectBuilderFactory(TestProjectFolders testProjectFolders, TestRunConfiguration testRunConfiguration, CurrentVersionDriver currentVersionDriver, ConfigurationGeneratorFactory configurationGeneratorFactory, BindingsGeneratorFactory bindingsGeneratorFactory, FeatureFileGenerator featureFileGenerator)
        {
            _testProjectFolders = testProjectFolders;
            _testRunConfiguration = testRunConfiguration;
            _currentVersionDriver = currentVersionDriver;
            _configurationGeneratorFactory = configurationGeneratorFactory;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _featureFileGenerator = featureFileGenerator;
        }

        public ProjectBuilder CreateProject(string language)
        {
            return CreateProjectInternal(null, ParseProgrammingLanguage(language));
        }

        public ProjectBuilder CreateProject(string projectName, string language)
        {
            return CreateProjectInternal(projectName, ParseProgrammingLanguage(language));
        }

        public ProjectBuilder CreateProject(string projectName, ProgrammingLanguage language)
        {
            return CreateProjectInternal(projectName, language);
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

        private ProjectBuilder CreateProjectInternal(string projectName, ProgrammingLanguage language)
        {
            var project = new ProjectBuilder(_testProjectFolders, _featureFileGenerator, _bindingsGeneratorFactory, _configurationGeneratorFactory, new Configuration(), _currentVersionDriver)
            {
                Language = language,
                Format = _testRunConfiguration.ProjectFormat,
                TargetFrameworks = _testRunConfiguration.TargetFramework,
            };

            project.Configuration.UnitTestProvider = _testRunConfiguration.UnitTestProvider;

            if (projectName.IsNotNullOrWhiteSpace())
            {
                project.ProjectName = projectName;
            }

            return project;
        }
    }
}
