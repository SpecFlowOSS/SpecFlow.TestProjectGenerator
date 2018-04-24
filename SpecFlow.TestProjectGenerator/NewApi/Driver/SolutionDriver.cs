using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class SolutionDriver
    {
        private readonly NuGetConfigGenerator _nuGetConfigGenerator;
        private readonly Folders _folders;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly Solution _solution;
        
        public SolutionDriver(NuGetConfigGenerator nuGetConfigGenerator, Folders folders, TestProjectFolders testProjectFolders)
        {
            _nuGetConfigGenerator = nuGetConfigGenerator;
            _folders = folders;
            _testProjectFolders = testProjectFolders;
            NuGetSources = new List<NuGetSource> { new NuGetSource("LocalSpecFlowDevPackages", _folders.NuGetFolder), new NuGetSource("LocalExternalPackages", _folders.ExternalNuGetFolder) };
            _solution = new Solution(SolutionName);
        }

        public Guid SolutionGuid { get; } = Guid.NewGuid();

        public IList<NuGetSource> NuGetSources { get; }

        public string SolutionName => $"TestSolution_{SolutionGuid:N}";

        public void AddProject(Project project)
        {
            _solution.AddProject(project);
        }

        public void WriteToDisk()
        {
            _solution.NugetConfig = _nuGetConfigGenerator?.Generate(NuGetSources.ToArray());

            var solutionWriter = new SolutionWriter();
            string solutionDirectoryPath = Path.Combine(_folders.FolderToSaveGeneratedSolutions, SolutionName);
            _testProjectFolders.PathToSolutionFile =  solutionWriter.WriteToFileSystem(_solution, solutionDirectoryPath);

            // TODO: find better solution
            var proj = _solution.Projects.Last();

            _testProjectFolders.ProjectFolder = Path.Combine(_testProjectFolders.PathToSolutionDirectory, proj.Name);
            _testProjectFolders.ProjectBinOutputPath = Path.Combine(_testProjectFolders.ProjectFolder, GetProjectCompilePath(proj));
            _testProjectFolders.TestAssemblyFileName = $"{proj.Name}.dll";
            _testProjectFolders.PathToNuGetPackages = proj.ProjectFormat == ProjectFormat.Old
                ? Path.Combine(_testProjectFolders.PathToSolutionDirectory, "packages")
                : _folders.GlobalPackages;
        }

        private string GetProjectCompilePath(Project project)
        {
            // TODO: hardcoded values should be replaced
            if (project.ProjectFormat == ProjectFormat.New)
            {
                return Path.Combine("bin", "Debug", project.TargetFrameworks.ToTargetFrameworkMoniker().Split(';')[0]);
            }

            return Path.Combine("bin", "Debug");
        }
    }
}
