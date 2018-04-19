using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class ProjectWriterFactory
    {
        public BaseProjectWriter FromProjectFormat(ProjectFormat projectFormat)
        {
            switch (projectFormat)
            {
                case ProjectFormat.Old:
                    return new OldFormatProjectWriter();
                case ProjectFormat.New:
                    return new NewFormatProjectWriter();
                default:
                    throw new ProjectCreationNotPossibleException("Unknown project format.");
            }
        }
    }
}
