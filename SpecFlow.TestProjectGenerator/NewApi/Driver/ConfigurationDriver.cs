using System;
using System.Collections.Generic;
using System.Globalization;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{

    public class Configuration
    {
        public UnitTestProvider UnitTestProvider { get; internal set; } = UnitTestProvider.XUnit;
        public List<SpecFlowPlugin> Plugins { get; } = new List<SpecFlowPlugin>();
        public List<AppConfigSection> AppConfigSection { get;  } = new List<AppConfigSection>() { new AppConfigSection(name: "specFlow", type: "TechTalk.SpecFlow.Configuration.ConfigurationSectionHandler, TechTalk.SpecFlow") };
        public List<StepAssembly> StepAssemblies { get;  } = new List<StepAssembly>();

        public CultureInfo FeatureLanguage { get; internal set; } = CultureInfo.GetCultureInfo("en-US");
        public CultureInfo BindingCulture { get; internal set; }

        public string GetUnitTestProviderName()
        {
            switch (UnitTestProvider)
            {
                case UnitTestProvider.SpecRunWithNUnit:
                    return "SpecRun+NUnit";
                case UnitTestProvider.SpecRunWithNUnit2:
                    return "SpecRun+NUnit.2";
                case UnitTestProvider.SpecRunWithMsTest:
                    return "SpecRun+MsTest";
                case UnitTestProvider.MSTest:
                    return "MSTest";
                case UnitTestProvider.NUnit2:
                    return "NUnit2";
                case UnitTestProvider.NUnit3:
                    return "NUnit";
                case UnitTestProvider.XUnit:
                    return "XUnit";
                case UnitTestProvider.SpecRun:
                    return "SpecRun";
                default:
                    throw new ArgumentOutOfRangeException(nameof(UnitTestProvider), UnitTestProvider, "value is not known");
            }
        }
    }

    public class ConfigurationDriver
    {
        private readonly Configuration _configuration;

        public ConfigurationDriver(Configuration configuration)
        {
            _configuration = configuration;
        }

        public void AddPlugin(SpecFlowPlugin specFlowPlugin)
        {
            _configuration.Plugins.Add(specFlowPlugin);
        }

        public void AddConfigSection(AppConfigSection appConfigSection)
        {
            _configuration.AppConfigSection.Add(appConfigSection);
        }

     

        public void SetUnitTestProvider(string providerName)
        {
            switch (providerName.ToLower())
            {
                case "specrun+nunit":
                    _configuration.UnitTestProvider = UnitTestProvider.SpecRunWithNUnit;
                    break;
                case "specrun+nunit.2":
                    _configuration.UnitTestProvider = UnitTestProvider.SpecRunWithNUnit2;
                    break;
                case "specrun+mstest":
                    _configuration.UnitTestProvider = UnitTestProvider.SpecRunWithMsTest;
                    break;
                case "specrun":
                    _configuration.UnitTestProvider = UnitTestProvider.SpecRun;
                    break;
                case "mstest":
                    _configuration.UnitTestProvider = UnitTestProvider.MSTest;
                    break;
                case "xunit":
                    _configuration.UnitTestProvider = UnitTestProvider.XUnit;
                    break;
                case "nunit":
                    _configuration.UnitTestProvider = UnitTestProvider.NUnit3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(providerName), providerName, "Unknown unit test provider");
            }
        }

        public void SetBindingCulture(CultureInfo bindingCulture)
        {
            _configuration.BindingCulture = bindingCulture;
        }
    }
}
