namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectFile  //FeatureFiles, Code, App.Config, NuGet.Config, packages.config,
    {
        private string v1;
        private string v2;
        private string v3;

        public ProjectFile(string v1, string v2, string v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public string Path { get;  } //relative from project
        public string Content { get; }

        public string BuildAction { get; }
    }
}