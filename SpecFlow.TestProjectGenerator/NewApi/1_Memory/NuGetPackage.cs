using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class NuGetPackage
    {
        public NuGetPackage(string name, string version, params (string PublicAssemblyName, string RelativeHintPath)[] assemblies)
        {
            Name = name;
            Version = version;
            Assemblies = new ReadOnlyCollection<(string, string)>(assemblies);
        }

        public string Name { get; }
        public string Version { get; }
        public IReadOnlyList<(string PublicAssemblyName, string RelativeHintPath)> Assemblies { get; }
    }
}