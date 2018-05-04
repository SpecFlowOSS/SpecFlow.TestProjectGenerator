using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class ProjectWriterFactory
    {
        private readonly IOutputWriter _outputWriter;

        public ProjectWriterFactory(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
        }

        public IProjectWriter FromProjectFormat(ProjectFormat projectFormat)
        {
            switch (projectFormat)
            {
                case ProjectFormat.Old:
                    return new OldFormatProjectWriter(_outputWriter);
                case ProjectFormat.New:
                    return new NewFormatProjectWriter(_outputWriter);
                default:
                    throw new ProjectCreationNotPossibleException("Unknown project format.");
            }
        }
    }
}
