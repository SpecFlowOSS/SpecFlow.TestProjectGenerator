using FluentAssertions;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using Xunit;

namespace SpecFlow.TestProjectGenerator.Tests
{
    public class NuGetConfigGeneratorTests
    {
        private readonly NuGetConfigGenerator _nuGetConfigGenerator;

        public NuGetConfigGeneratorTests()
        {
            _nuGetConfigGenerator = new NuGetConfigGenerator();
        }

        [Fact]
        public void FileNameIsAppConfig()
        {
            var projectFile = _nuGetConfigGenerator.Generate();
            projectFile.Path.Should().Be("nuget.config");
        }

        [Fact]
        public void BuildActionIsNone()
        {
            var projectFile = _nuGetConfigGenerator.Generate();
            projectFile.BuildAction.Should().Be("None");
        }


        [Fact]
        public void NoSourcesAdded()
        {
            var projectFile = _nuGetConfigGenerator.Generate();
            projectFile.Content.Should()
                .Be(
                    "<configuration><packageSources><add key=\"Nuget.org\" value=\"https://api.nuget.org/v3/index.json\" /></packageSources></configuration>");
        }

        [Fact]
        public void OneSourceAdded()
        {
            var projectFile = _nuGetConfigGenerator.Generate(new NuGetSource[] { new NuGetSource("localPackages", "LocalPackages") });
            projectFile.Content.Should()
                .Be(
                    "<configuration><packageSources><add key=\"localPackages\" value=\"LocalPackages\" /><add key=\"Nuget.org\" value=\"https://api.nuget.org/v3/index.json\" /></packageSources></configuration>");
        }

        [Fact]
        public void TwoSourcesAdded()
        {
            var projectFile = _nuGetConfigGenerator.Generate(new NuGetSource[]
            {
                new NuGetSource("localPackages", "LocalPackages"),
                new NuGetSource("Gherkin CI", "https://ci.appveyor.com/nuget/gherkin-pgwyovw4bucb/"),
            });
            projectFile.Content.Should()
                .Be(
                    "<configuration><packageSources><add key=\"localPackages\" value=\"LocalPackages\" /><add key=\"Gherkin CI\" value=\"https://ci.appveyor.com/nuget/gherkin-pgwyovw4bucb/\" /><add key=\"Nuget.org\" value=\"https://api.nuget.org/v3/index.json\" /></packageSources></configuration>");
        }
    }


}
