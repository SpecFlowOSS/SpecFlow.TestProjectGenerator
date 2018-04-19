using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class SolutionDriver
    {
        private readonly NuGetConfigGenerator _nuGetConfigGenerator;
        private readonly Folders _folders;
        private readonly TestProjectFolders _testProjectFolders;
        private Solution _solution;
        public Guid SolutionGuid { get; } = Guid.NewGuid();
        public string SolutionName => $"TestSolution_{SolutionGuid:N}";


        public SolutionDriver(NuGetConfigGenerator nuGetConfigGenerator, Folders folders, TestProjectFolders testProjectFolders)
        {
            _nuGetConfigGenerator = nuGetConfigGenerator;
            _folders = folders;
            _testProjectFolders = testProjectFolders;
            _solution = new Solution(SolutionName);

            var nugetConfig = _nuGetConfigGenerator.Generate(new NuGetSource[] {new NuGetSource("LocalSpecFlowDevPackages", _folders.NuGetFolder)});
            _solution.NugetConfig = nugetConfig;
        }

        public void AddProject(Project project)
        {
            _solution.AddProject(project);
        }

        public void WriteToDisk()
        {
            var solutionWriter = new SolutionWriter();

            string solutionDirectoryPath = Path.Combine(_folders.FolderToSaveGeneratedSolutions, SolutionName);



            _testProjectFolders.PathToSolutionFile =  solutionWriter.WriteToFileSystem(_solution, solutionDirectoryPath);

        }
    }
}
