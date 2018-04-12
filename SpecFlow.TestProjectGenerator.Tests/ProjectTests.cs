using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using SpecFlow.TestProjectGenerator.NewApi._2_Filesystem;
using Xunit;

namespace SpecFlow.TestProjectGenerator.Tests
{
    public class ProjectTests
    {
        public (Solution, Project, string) CreateEmptySolutionAndProject(ProjectFormat projectFormat, ProgrammingLanguage programmingLanguage, string targetFramework)
        {
            var folder = Path.Combine(Path.GetTempPath(), "SpecFlow.TestProjectGenerator.Tests", Guid.NewGuid().ToString("N"));

            var solution = new Solution("SolutionName");
            var project = new Project("ProjectName", programmingLanguage, targetFramework, projectFormat);

            solution.AddProject(project);

            return (solution, project, folder);
        }

        [Fact]
        public void AddNuGetPackageToProjectInNewFormat()
        {
            
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, "net45");


            project.AddNuGetPackage("SpecFlow", "2.3.1");


            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<PackageReference Include=\"SpecFlow\" Version=\"2.3.1\" />");
        }

        [Fact]
        public void AddNuGetPackageToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp, "net45");


            project.AddNuGetPackage("SpecFlow", "2.3.1");


            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<Import Project=\"..\\packages\\SpecFlow.2.3.1\\build\\SpecFlow.targets\" Condition=\"Exists(\'..\\packages\\SpecFlow.2.3.1\\build\\SpecFlow.targets\')\" />");
            projectFileContent.Should().Contain("<Reference Include=\"TechTalk.SpecFlow, Version=2.3.1.0, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL\">\r\n      <HintPath>..\\packages\\SpecFlow.2.3.1\\lib\\net45\\TechTalk.SpecFlow.dll</HintPath>\r\n    </Reference>");
        }


        [Fact]
        public void AddNuGetPackageWithMSBuildFilesToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp, "net45");


            project.AddNuGetPackage("SpecFlow.Tools.MsBuild.Generation", "2.3.2-preview20180328");


            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<Import Project=\"..\\packages\\SpecFlow.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\SpecFlow.Tools.MsBuild.Generation.props\" Condition=\"Exists(\'..\\packages\\SpecFlow.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\SpecFlow.Tools.MsBuild.Generation.props\')\" />");
            projectFileContent.Should().Contain("<Import Project=\"..\\packages\\SpecFlow.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\SpecFlow.Tools.MsBuild.Generation.targets\" Condition=\"Exists(\'..\\packages\\SpecFlow.Tools.MsBuild.Generation.2.3.2-preview20180328\\build\\SpecFlow.Tools.MsBuild.Generation.targets\')\" />");
            
        }

        [Fact]
        public void AddReferenceToProjectInNewFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, "net45");


            project.AddReference("System.Configuration");


            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<Reference Include=\"System.Configuration\" />");
        }

        [Fact]
        public void AddReferenceToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp, "net45");


            project.AddReference("System.Configuration");


            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);


            projectFileContent.Should().Contain("<Reference Include=\"System.Configuration\" />");
        }


        [Fact]
        public void AddFileToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp, "net45");

            var projectFile = new ProjectFile("File.cs", "Compile", "//no code");

            project.AddFile(projectFile);


            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);
            var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "File.cs");


            projectFileContent.Should().Contain("<Compile Include=\"File.cs\" />");
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Contain("//no code");

        }

        [Fact]
        public void AddFileToProjectInNewFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp, "net45");

            var projectFile = new ProjectFile("File.cs", "Compile", "//no code");

            project.AddFile(projectFile);


            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);
            var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "File.cs");


            projectFileContent.Should().NotContain("<Compile");
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Contain("//no code");

        }

        [Fact]
        public void AddFileInFolderToProjectInOldFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp, "net45");

            var projectFile = new ProjectFile(Path.Combine("Folder","File.cs"), "Compile", "//no code");

            project.AddFile(projectFile);


            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);
            var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "Folder", "File.cs");


            projectFileContent.Should().Contain("<Compile Include=\"Folder\\File.cs\" />");
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Contain("//no code");

        }

        [Fact]
        public void AddFileInFolderToProjectInNewFormat()
        {

            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp, "net45");

            var projectFile = new ProjectFile(Path.Combine("Folder", "File.cs"), "Compile", "//no code");

            project.AddFile(projectFile);


            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);
            var filePath = Path.Combine(GetProjectFolderPath(solutionFolder, project), "Folder", "File.cs");


            projectFileContent.Should().NotContain("<Compile");
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Contain("//no code");

        }

        private string GetProjectFileContent(string solutionFolder, Project project)
        {
            return File.ReadAllText(Path.Combine(GetProjectFolderPath(solutionFolder, project), $"{project.Name}.csproj"));
        }

        private string GetProjectFolderPath(string solutionFolder, Project project)
        {
            return Path.Combine(solutionFolder, project.Name);
        }

        [Theory]
        [InlineData(ProgrammingLanguage.CSharp)]
        [InlineData(ProgrammingLanguage.VB)]
        [InlineData(ProgrammingLanguage.FSharp)]
        public void CreateEmtpyProjectInNewFormat(ProgrammingLanguage programmingLanguage)
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, programmingLanguage, "net45");

            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should().Be("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <PropertyGroup>\r\n    <TargetFrameworks>net45</TargetFrameworks>\r\n  </PropertyGroup>\r\n\r\n</Project>\r\n");

        }

        [Fact]
        public void CreateEmtpyProjectWithMultipleFrameworksInNewFormat()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.New, ProgrammingLanguage.CSharp, "net45;netstandard2.0");

            new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            var projectFileContent = GetProjectFileContent(solutionFolder, project);

            projectFileContent.Should().Be("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <PropertyGroup>\r\n    <TargetFrameworks>net45;netstandard2.0</TargetFrameworks>\r\n  </PropertyGroup>\r\n\r\n</Project>\r\n");
        }

        [Fact]
        public void MultipleFrameworksInOldFormatNotPossible()
        {
            var (solution, project, solutionFolder) = CreateEmptySolutionAndProject(ProjectFormat.Old, ProgrammingLanguage.CSharp, "net45;netstandard2.0");

            Action createSolution = () => new SolutionWriter().WriteToFileSystem(solution, solutionFolder);

            createSolution.ShouldThrow<ProjectCreationNotPossibleException>().WithMessage("Multiple target frameworks don't work with the old csproj format");


        }
    }

    
}
