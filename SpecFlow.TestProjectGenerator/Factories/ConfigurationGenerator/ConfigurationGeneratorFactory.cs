using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.Factories.ConfigurationGenerator
{
    public class ConfigurationGeneratorFactory
    {
        private readonly AppConfigGenerator _appConfigGenerator;
        private readonly JsonConfigGenerator _jsonConfigGenerator;

        public ConfigurationGeneratorFactory(AppConfigGenerator appConfigGenerator, JsonConfigGenerator jsonConfigGenerator)
        {
            this._appConfigGenerator = appConfigGenerator;
            this._jsonConfigGenerator = jsonConfigGenerator;
        }
        public IConfigurationGenerator FromConfigurationFormat(ConfigurationFormat configurationFormat)
        {
            switch (configurationFormat)
            {
                case ConfigurationFormat.Config: return _appConfigGenerator;
                case ConfigurationFormat.Json: return _jsonConfigGenerator;
                default: throw new ArgumentOutOfRangeException(nameof(configurationFormat));
            }
        }
    }
}