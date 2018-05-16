using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator
{
    public class ConfigurationGeneratorFactory
    {
        public IConfigurationGenerator FromConfigurationFormat(ConfigurationFormat configurationFormat)
        {
            switch (configurationFormat)
            {
                case ConfigurationFormat.Config: return new AppConfigGenerator();
                case ConfigurationFormat.Json: return new JsonConfigGenerator();
                default: throw new ArgumentOutOfRangeException(nameof(configurationFormat));
            }
        }
    }
}