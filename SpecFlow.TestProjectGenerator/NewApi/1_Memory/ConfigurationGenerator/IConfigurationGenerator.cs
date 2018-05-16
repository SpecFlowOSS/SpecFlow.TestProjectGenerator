using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationModel;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator
{
    public interface IConfigurationGenerator
    {
        ProjectFile Generate(Configuration configuration);
    }
}
