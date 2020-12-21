namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class CucumberMessagesConfigurationDriver
    {
        public void SetEnabled(ProjectBuilder project, bool? isEnabled)
        {
            project.Configuration.CucumberMessagesSection.Enabled = isEnabled;
        }
    }
}
