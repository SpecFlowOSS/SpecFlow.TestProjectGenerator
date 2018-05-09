using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi
{
    public class TestRunConfiguration
    {
        public TestRunConfiguration(ProgrammingLanguage programmingLanguage, ProjectFormat projectFormat, TargetFramework targetFramework)
        {
            ProgrammingLanguage = programmingLanguage;
            ProjectFormat = projectFormat;
            TargetFramework = targetFramework;
        }

        public ProgrammingLanguage ProgrammingLanguage { get; set; }
        public ProjectFormat ProjectFormat { get; set; }

        public TargetFramework TargetFramework { get; set; }
    }
}