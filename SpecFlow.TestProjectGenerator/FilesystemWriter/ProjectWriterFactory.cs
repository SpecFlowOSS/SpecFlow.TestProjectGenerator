using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class ProjectWriterFactory
    {
        private readonly IOutputWriter _outputWriter;
        private readonly TargetFrameworkMonikerStringBuilder _targetFrameworkMonikerStringBuilder;

        public ProjectWriterFactory(IOutputWriter outputWriter, TargetFrameworkMonikerStringBuilder targetFrameworkMonikerStringBuilder)
        {
            _outputWriter = outputWriter;
            _targetFrameworkMonikerStringBuilder = targetFrameworkMonikerStringBuilder;
        }

        public IProjectWriter FromProjectFormat(ProjectFormat projectFormat)
        {
            switch (projectFormat)
            {
                case ProjectFormat.Old:
                    return new OldFormatProjectWriter(_outputWriter, _targetFrameworkMonikerStringBuilder);
                case ProjectFormat.New:
                    return new NewFormatProjectWriter(_outputWriter, _targetFrameworkMonikerStringBuilder);
                default:
                    throw new ProjectCreationNotPossibleException("Unknown project format.");
            }
        }
    }
}
