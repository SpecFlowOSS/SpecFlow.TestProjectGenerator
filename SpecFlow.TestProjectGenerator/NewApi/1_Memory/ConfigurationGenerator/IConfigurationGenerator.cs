using SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationModel;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator
{
    public interface IConfigurationGenerator
    {
        ProjectFile Generate(Configuration configuration);
    }
}
