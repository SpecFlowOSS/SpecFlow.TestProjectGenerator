namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet
{
    public partial class AddCommandBuilder
    {
        internal static AddCommandBuilder Create() => new AddCommandBuilder();

        public AddPackageCommandBuilder Package() => new AddPackageCommandBuilder();

        public AddReferenceCommandBuilder Reference() => new AddReferenceCommandBuilder();
    }
}
