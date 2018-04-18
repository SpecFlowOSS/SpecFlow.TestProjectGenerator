using System.Configuration;

namespace SpecFlow.TestProjectGenerator
{
    public class AppConfigDriver
    {
        public string ProjectName => ConfigurationManager.AppSettings["testProjectFolder"] ?? "TestProject";
        public string VSTestPath => ConfigurationManager.AppSettings["vstestPath"] ?? "Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow";
    }
}