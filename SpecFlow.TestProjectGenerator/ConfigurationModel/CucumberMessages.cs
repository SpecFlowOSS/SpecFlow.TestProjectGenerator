using System.Collections.Generic;

namespace TechTalk.SpecFlow.TestProjectGenerator.ConfigurationModel
{
    public class CucumberMessages
    {
        public bool? Enabled { get; set; }

        public List<CucumberMessagesSink> Sinks { get; set; } = new List<CucumberMessagesSink>();
    }
}
