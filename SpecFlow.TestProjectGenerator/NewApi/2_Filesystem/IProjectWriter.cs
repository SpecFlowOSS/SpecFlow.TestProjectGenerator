using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public interface IProjectWriter
    {
        string WriteProject(Project project, string path);
    }
}
