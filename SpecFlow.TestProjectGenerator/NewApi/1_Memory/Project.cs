using System.Collections.Generic;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class Project
    {
        
        public Project(string v1, ProgrammingLanguage cSharp, string v2, ProjectFormat old)
        {
        }

        public string Name { get; set; }
        public string TargetFrameworks { get; set; } //net45, netcoreapp1.1, net471,

        public ProgrammingLanguage ProgrammingLanguage { get; set; }

        public IReadOnlyList<NuGetPackage> NuGetPackages { get;  }
        public IReadOnlyList<ProjectReference> ProjectReferences { get; }
        public IReadOnlyList<Reference> References { get; }

        public IReadOnlyList<ProjectFile> Files { get; }

        public void AddNuGetPackage(string specflow, string s)
        {
            throw new System.NotImplementedException();
        }

        public void AddReference(string systemConfiguration)
        {
            throw new System.NotImplementedException();
        }

        public void AddFile(ProjectFile projectFile)
        {
            throw new System.NotImplementedException();
        }
    }
}
