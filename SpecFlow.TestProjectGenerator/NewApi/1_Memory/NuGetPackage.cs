using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class NuGetPackage
    {
        public NuGetPackage(string name, string version, params NuGetPackageAssembly[] assemblies)
        {
            Name = name;
            Version = version;
            Assemblies = new ReadOnlyCollection<NuGetPackageAssembly>(assemblies);
        }

        public string Name { get; }
        public string Version { get; }
        public IReadOnlyList<NuGetPackageAssembly> Assemblies { get; }
    }
}