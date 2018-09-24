namespace TechTalk.SpecFlow.TestProjectGenerator.Dotnet
{
    public class BuildCommandBuilder : BaseCommandBuilder
    {
        internal static BuildCommandBuilder Create(IOutputWriter outputWriter)
        {
            return new BuildCommandBuilder(outputWriter);
        }

        protected override string BuildArguments()
        {
            return "build";
        }

        public BuildCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
        {
        }
    }

}
