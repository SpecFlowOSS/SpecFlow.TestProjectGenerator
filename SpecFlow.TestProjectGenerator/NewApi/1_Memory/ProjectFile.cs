namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectFile  //FeatureFiles, Code, App.Config, NuGet.Config, packages.config,
    {
        public ProjectFile(string path, string buildAction, string content, CopyToOutputDirectory copyToOutputDirectory = CopyToOutputDirectory.DoNotCopy)
        {
            Path = path;
            Content = content;
            BuildAction = buildAction;
            CopyToOutputDirectory = copyToOutputDirectory;
        }

        public string Path { get; } //relative from project
        public string Content { get; }
        public string BuildAction { get; }
        public CopyToOutputDirectory CopyToOutputDirectory { get; }
    }

    public enum CopyToOutputDirectory
    {
        CopyIfNewer,
        CopyAlways,
        DoNotCopy
    }
}