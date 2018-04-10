namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    internal class NuGetPackage
    {
        public NuGetPackage(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; private set; }
        public string Version { get; private set; }
    }
}