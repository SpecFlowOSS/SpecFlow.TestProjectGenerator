using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class Project
    {
        private readonly List<NuGetPackage> _nuGetPackages = new List<NuGetPackage>();

        private readonly List<ProjectReference> _projectReferences = new List<ProjectReference>();

        private readonly List<Reference> _references = new List<Reference>();

        private readonly List<ProjectFile> _files = new List<ProjectFile>();

        public Project(string name, ProgrammingLanguage programmingLanguage, TargetFramework targetFrameworks, ProjectFormat projectFormat, ProjectType projectType = ProjectType.Library)
        {
            ProgrammingLanguage = programmingLanguage;
            ProjectFormat = projectFormat;
            ProjectType = projectType;
            NuGetPackages = new ReadOnlyCollection<NuGetPackage>(_nuGetPackages);
            ProjectReferences = new ReadOnlyCollection<ProjectReference>(_projectReferences);
            References = new ReadOnlyCollection<Reference>(_references);
            Files = new ReadOnlyCollection<ProjectFile>(_files);
            Name = name;
            TargetFrameworks = targetFrameworks;
        }
        
        public string Name { get; }
        public TargetFramework TargetFrameworks { get; } //net45, netcoreapp1.1, net471,
        public ProjectType ProjectType { get; }

        public ProgrammingLanguage ProgrammingLanguage { get; }
        public ProjectFormat ProjectFormat { get; }

        public IReadOnlyList<NuGetPackage> NuGetPackages { get; }
        public IReadOnlyList<ProjectReference> ProjectReferences { get; }
        public IReadOnlyList<Reference> References { get; }
        public IReadOnlyList<ProjectFile> Files { get; }

        public void AddNuGetPackage(string name, string version)
        {
            _nuGetPackages.Add(new NuGetPackage(name, version));
        }

        public void AddReference(string name)
        {
            _references.Add(new Reference(name));
        }

        public void AddFile(ProjectFile projectFile)
        {
            _files.Add(projectFile ?? throw new ArgumentNullException(nameof(projectFile)));
        }

        public void AddProjectReference(string fullPath)
        {
            _projectReferences.Add(new ProjectReference(fullPath));
        }
    }
}
