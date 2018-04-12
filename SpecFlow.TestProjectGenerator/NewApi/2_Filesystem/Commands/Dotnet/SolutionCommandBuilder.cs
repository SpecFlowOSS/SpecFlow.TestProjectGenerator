namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet
{
    public partial class SolutionCommandBuilder

    {
        public static SolutionCommandBuilder Create() => new SolutionCommandBuilder();

        public AddProjectSolutionCommandBuilder AddProject() => new AddProjectSolutionCommandBuilder(this);
    }
}
