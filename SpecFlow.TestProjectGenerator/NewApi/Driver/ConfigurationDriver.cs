using System;
using System.Globalization;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class ConfigurationDriver
    {
        private readonly ProjectsDriver _projectsDriver;

        public ConfigurationDriver(ProjectsDriver projectsDriver)
        {
            _projectsDriver = projectsDriver;
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

        public void SetIsRowTestsAllowed(string projectName, bool isAllowed)
        {
            var project = _projectsDriver.Projects[projectName];
            SetIsRowTestsAllowed(project, isAllowed);
        }

        public void SetIsRowTestsAllowed(bool isAllowed)
        {
            SetIsRowTestsAllowed(_projectsDriver.DefaultProject, isAllowed);
        }

        public void AddGeneratorRegisterDependency(string type, string @as) => AddGeneratorRegisterDependency(_projectsDriver.DefaultProject, type, @as);
        public void AddGeneratorRegisterDependency(string type, string @as, string name) => AddGeneratorRegisterDependency(_projectsDriver.DefaultProject, type, @as, name);
        public void AddGeneratorRegisterDependency(string projectName, string type, string @as, string name) => AddGeneratorRegisterDependency(_projectsDriver.Projects[projectName], type, @as, name);

        public void AddRuntimeRegisterDependency(string type, string @as) => AddRuntimeRegisterDependency(_projectsDriver.DefaultProject, type, @as);
        public void AddRuntimeRegisterDependency(string type, string @as, string name) => AddRuntimeRegisterDependency(_projectsDriver.DefaultProject, type, @as, name);
        public void AddRuntimeRegisterDependency(string projectName, string type, string @as, string name) => AddRuntimeRegisterDependency(_projectsDriver.Projects[projectName], type, @as, name);

        public void AddGeneratorRegisterDependency(ProjectBuilder project, string type, string @as)
        {
            project.Configuration.Generator.Value.AddRegisterDependency(type, @as);
        }
        public void AddGeneratorRegisterDependency(ProjectBuilder project, string type, string @as, string name)
        {
            project.Configuration.Generator.Value.AddRegisterDependency(type, @as, name);
        }

        public void AddRuntimeRegisterDependency(ProjectBuilder project, string type, string @as, string name)
        {
            project.Configuration.Runtime.Value.AddRegisterDependency(type, @as, name);
        }

        public void AddRuntimeRegisterDependency(ProjectBuilder project, string type, string @as)
        {
            project.Configuration.Runtime.Value.AddRegisterDependency(type, @as);
        }

        public void SetBindingCulture(ProjectBuilder project, CultureInfo bindingCulture)
        {
            project.Configuration.BindingCulture = bindingCulture;
        }

        public void SetUnitTestProvider(ProjectBuilder project, string unitTestProviderName)
        {
            project.Configuration.UnitTestProvider = GetUnitTestProvider(unitTestProviderName);
        }

        public void AddStepAssembly(ProjectBuilder project, StepAssembly stepAssembly)
        {
            project.Configuration.StepAssemblies.Add(stepAssembly);
        }

        public void SetConfigurationFormat(ProjectBuilder project, ConfigurationFormat configurationFormat)
        {
            project.ConfigurationFormat = configurationFormat;
        }

        public void SetIsRowTestsAllowed(ProjectBuilder project, bool isAllowed)
        {
            project.Configuration.Generator.Value.AllowRowTests = isAllowed;
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
