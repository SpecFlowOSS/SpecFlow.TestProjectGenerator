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

        public Project(string name, Guid projectGuid, ProgrammingLanguage programmingLanguage, TargetFramework targetFrameworks, ProjectFormat projectFormat, ProjectType projectType = ProjectType.Library)
        {
            ProgrammingLanguage = programmingLanguage;
            ProjectFormat = projectFormat;
            ProjectType = projectType;
            Name = name;
            ProjectGuid = projectGuid;
            TargetFrameworks = targetFrameworks;

            if (projectFormat == ProjectFormat.Old)
            {
                AddReference("System");
                AddReference("System.Configuration");
                AddReference("System.Core");
            }
        }
        
        public string Name { get; }
        public Guid ProjectGuid { get; }
        public TargetFramework TargetFrameworks { get; } //net45, netcoreapp1.1, net471,
        public ProjectType ProjectType { get; }

        public ProgrammingLanguage ProgrammingLanguage { get; }
        public ProjectFormat ProjectFormat { get; }

        public IReadOnlyList<NuGetPackage> NuGetPackages => _nuGetPackages;
        public IReadOnlyList<ProjectReference> ProjectReferences => _projectReferences;
        public IReadOnlyList<Reference> References => _references;
        public IReadOnlyList<ProjectFile> Files => _files;

        public void AddNuGetPackage(string name, string version, params NuGetPackageAssembly[] assemblies)
        {
            _nuGetPackages.Add(new NuGetPackage(name, version, assemblies));
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
