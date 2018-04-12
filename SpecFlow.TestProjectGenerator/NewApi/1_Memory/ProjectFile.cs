namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectFile  //FeatureFiles, Code, App.Config, NuGet.Config, packages.config,
    {
        public ProjectFile(string path, string content, string buildAction)
        {
            Path = path;
            Content = content;
            BuildAction = buildAction;
        }

        public string Path { get; } //relative from project
        public string Content { get; }
        public string BuildAction { get; }
    }
}