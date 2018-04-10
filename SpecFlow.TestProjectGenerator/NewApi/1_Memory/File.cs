namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    internal class File  //FeatureFiles, Code, App.Config, NuGet.Config, packages.config,
    {
        public string Path { get;  } //relative from project
        public string Content { get; }

        public string BuildAction { get; }
    }
}