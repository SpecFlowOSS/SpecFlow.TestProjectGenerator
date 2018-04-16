namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet
{
    public class DotNet
    {
        public static NewCommandBuilder New() => NewCommandBuilder.Create();
        public static BuildCommandBuilder Build() => BuildCommandBuilder.Create();
        public static SolutionCommandBuilder Sln() => SolutionCommandBuilder.Create();
        public static VersionCommandBuilder Version() => VersionCommandBuilder.Create();
        public static AddCommandBuilder Add() => AddCommandBuilder.Create();
    }
}
