using System;
using System.Globalization;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Configuration.AppConfig;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class ConfigurationDriver
    {
        private readonly ProjectsDriver _projectsDriver;

        public ConfigurationDriver(ProjectsDriver projectsDriver)
        {
            _projectsDriver = projectsDriver;
        }

        public void AddFromXmlSpecFlowSection(string projectName, string specFlowSection)
        {
            var project = _projectsDriver.Projects[projectName];
            AddFromXmlSpecFlowSection(project, specFlowSection);
        }

        public void AddFromXmlSpecFlowSection(string specFlowSection)
        {
            AddFromXmlSpecFlowSection(_projectsDriver.DefaultProject, specFlowSection);
        }

        public void AddPlugin(string projectName, SpecFlowPlugin specFlowPlugin)
        {
            _projectsDriver.Projects[projectName].Configuration.Plugins.Add(specFlowPlugin);
        }

        public void AddPlugin(SpecFlowPlugin specFlowPlugin)
        {
            _projectsDriver.DefaultProject.Configuration.Plugins.Add(specFlowPlugin);
        }

        public void AddStepAssembly(string projectName, StepAssembly stepAssembly)
        {
            var project = _projectsDriver.Projects[projectName];
            AddStepAssembly(project, stepAssembly);
        }

        public void AddStepAssembly(StepAssembly stepAssembly)
        {
            AddStepAssembly(_projectsDriver.DefaultProject, stepAssembly);
        }

        public void AddConfigSection(string projectName, AppConfigSection appConfigSection)
        {
            _projectsDriver.Projects[projectName].Configuration.AppConfigSection.Add(appConfigSection);
        }

        public void AddConfigSection(AppConfigSection appConfigSection)
        {
            _projectsDriver.DefaultProject.Configuration.AppConfigSection.Add(appConfigSection);
        }

        public void SetUnitTestProvider(string projectName, string unitTestProviderName)
        {
            var project = _projectsDriver.Projects[projectName];
            SetUnitTestProvider(project, unitTestProviderName);
        }

        public void SetUnitTestProvider(string unitTestProviderName)
        {
            SetUnitTestProvider(_projectsDriver.DefaultProject, unitTestProviderName);
            _projectsDriver.DefaultProject.Configuration.UnitTestProvider = GetUnitTestProvider(unitTestProviderName);
        }

        public void SetBindingCulture(string projectName, CultureInfo bindingCulture)
        {
            var project = _projectsDriver.Projects[projectName];
            SetBindingCulture(project, bindingCulture);
        }

        public void SetBindingCulture(CultureInfo bindingCulture)
        {
            SetBindingCulture(_projectsDriver.DefaultProject, bindingCulture);
        }

        public void SetConfigurationFormat(ConfigurationFormat configurationFormat)
        {
            SetConfigurationFormat(_projectsDriver.DefaultProject, configurationFormat);
        }

        public void SetConfigurationFormat(string projectName, ConfigurationFormat configurationFormat)
        {
            var project = _projectsDriver.Projects[projectName];
            SetConfigurationFormat(project, configurationFormat);
        }

        private void SetBindingCulture(ProjectBuilder project, CultureInfo bindingCulture)
        {
            project.Configuration.BindingCulture = bindingCulture;
        }

        private void SetUnitTestProvider(ProjectBuilder project, string unitTestProviderName)
        {
            project.Configuration.UnitTestProvider = GetUnitTestProvider(unitTestProviderName);
        }

        private void AddStepAssembly(ProjectBuilder project, StepAssembly stepAssembly)
        {
            project.Configuration.StepAssemblies.Add(stepAssembly);
        }

        private void SetConfigurationFormat(ProjectBuilder project, ConfigurationFormat configurationFormat)
        {
            project.ConfigurationFormat = configurationFormat;
        }

        private void AddFromXmlSpecFlowSection(ProjectBuilder project, string specFlowSection)
        {
            var configSection = ConfigurationSectionHandler.CreateFromXml(specFlowSection);
            var appConfigConfigurationLoader = new AppConfigConfigurationLoader();

            var specFlowConfiguration = appConfigConfigurationLoader.LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);

            foreach (string stepAssemblyName in specFlowConfiguration.AdditionalStepAssemblies)
            {
                AddStepAssembly(project, new StepAssembly(stepAssemblyName));
            }

            SetUnitTestProvider(project, specFlowConfiguration.UnitTestProvider);
            SetBindingCulture(project, specFlowConfiguration.BindingCulture);
        }

        private UnitTestProvider GetUnitTestProvider(string providerName)
        {
            switch (providerName.ToLower())
            {
                case "specrun+nunit": return UnitTestProvider.SpecRunWithNUnit;
                case "specrun+nunit.2": return UnitTestProvider.SpecRunWithNUnit2;
                case "specrun+mstest": return UnitTestProvider.SpecRunWithMsTest;
                case "specrun": return UnitTestProvider.SpecRun;
                case "mstest": return UnitTestProvider.MSTest;
                case "xunit": return UnitTestProvider.XUnit;
                case "nunit": return UnitTestProvider.NUnit3;
                default: throw new ArgumentOutOfRangeException(nameof(providerName), providerName, "Unknown unit test provider");
            }
        }
    }
}
