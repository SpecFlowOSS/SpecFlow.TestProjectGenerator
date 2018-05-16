namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class NuGetPackageAssembly
    {
        public NuGetPackageAssembly(string publicAssemblyName, string relativeHintPath)
        {
            PublicAssemblyName = publicAssemblyName;
            RelativeHintPath = relativeHintPath;
        }

        public string PublicAssemblyName { get; }
        public string RelativeHintPath { get; }
    }
}
