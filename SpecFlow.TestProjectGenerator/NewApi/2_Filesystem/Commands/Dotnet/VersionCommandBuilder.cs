namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet
{
    public class VersionCommandBuilder : BaseCommandBuilder
    {
        public static VersionCommandBuilder Create()
        {
            return new VersionCommandBuilder();
        }

        protected override string BuildArguments()
        {
            return "--version";
        }
    }
}
