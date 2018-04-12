namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class NuGetPackage
    {
        public NuGetPackage(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }
        public string Version { get; }
    }
}