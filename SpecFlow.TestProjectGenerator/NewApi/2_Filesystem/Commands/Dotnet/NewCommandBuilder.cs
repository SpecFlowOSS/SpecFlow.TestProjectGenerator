namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet
{
    public partial class NewCommandBuilder
    {
        internal static NewCommandBuilder Create() => new NewCommandBuilder();

        public NewSolutionCommandBuilder Solution() => new NewSolutionCommandBuilder(this);

        public NewProjectCommandBuilder Project() => new NewProjectCommandBuilder(this);
    }
}
