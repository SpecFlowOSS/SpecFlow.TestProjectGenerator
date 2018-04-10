using System.Collections.Generic;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    class Project
    {
        public string Name { get; set; }
        public string TargetFrameworks { get; set; } //net45, netcoreapp1.1, net471,

        public ProgrammingLanguage ProgrammingLanguage { get; set; }

        public IReadOnlyList<NuGetPackage> NuGetPackages { get;  }
        public IReadOnlyList<ProjectReference> ProjectReferences { get; }
        public IReadOnlyList<Reference> References { get; }

        public IReadOnlyList<File> Files { get; } 
    }
}
