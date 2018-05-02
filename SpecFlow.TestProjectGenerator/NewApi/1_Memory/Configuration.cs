using System.Collections.Generic;
using System.Globalization;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class Configuration
    {
        public UnitTestProvider UnitTestProvider { get; set; } = UnitTestProvider.XUnit;
        public List<SpecFlowPlugin> Plugins { get; } = new List<SpecFlowPlugin>();
        public List<AppConfigSection> AppConfigSection { get;  } = new List<AppConfigSection> { new AppConfigSection(name: "specFlow", type: "TechTalk.SpecFlow.Configuration.ConfigurationSectionHandler, TechTalk.SpecFlow") };
        public List<StepAssembly> StepAssemblies { get;  } = new List<StepAssembly>();
        public CultureInfo FeatureLanguage { get; set; } = CultureInfo.GetCultureInfo("en-US");
        public CultureInfo BindingCulture { get; set; }
    }
}