using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public interface IProjectWriter
    {
        string WriteProject(Project project, string path);

        void WriteReferences(Project project, string projectFilePath);
    }
}
