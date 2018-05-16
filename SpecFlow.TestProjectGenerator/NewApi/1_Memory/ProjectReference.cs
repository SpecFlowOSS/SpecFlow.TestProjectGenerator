namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectReference
    {
        public ProjectReference(string path, ProjectBuilder project)
        {
            Path = path;
            Project = project;
        }

        public string Path { get; }

        public ProjectBuilder Project { get; }
    }
}