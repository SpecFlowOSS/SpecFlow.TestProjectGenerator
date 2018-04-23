using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
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
        }
    }
}
