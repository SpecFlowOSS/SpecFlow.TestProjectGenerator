namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet
{
    public class BuildCommandBuilder : BaseCommandBuilder
    {
        internal static BuildCommandBuilder Create()
        {
            return new BuildCommandBuilder();
        }

        protected override string BuildArguments()
        {
            return "build";
        }
    }

}
