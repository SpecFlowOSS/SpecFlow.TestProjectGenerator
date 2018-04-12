namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet
{
    public abstract class BaseCommandBuilder
    {
        private const string ExecutablePath = "dotnet";

        public CommandBuilder Build()
        {
            return new CommandBuilder(ExecutablePath, BuildArguments());
        }

        protected abstract string BuildArguments();

        protected string AddArgument(string argumentsFormat, string option, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                argumentsFormat += $" {option} {value}";
            }

            return argumentsFormat;
        }
    }
}
