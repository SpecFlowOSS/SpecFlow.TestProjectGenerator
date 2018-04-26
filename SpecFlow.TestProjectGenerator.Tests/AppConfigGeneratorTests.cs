using FluentAssertions;
using SpecFlow.TestProjectGenerator.NewApi.Driver;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using Xunit;

namespace SpecFlow.TestProjectGenerator.Tests
{
    public class AppConfigGeneratorTests
    {
        private readonly AppConfigGenerator _appConfigGenerator;

        public AppConfigGeneratorTests()
        {
            _appConfigGenerator = new AppConfigGenerator();
        }

        [Fact]
        public void FileNameIsAppConfig()
        {
            var configuration = new Configuration {UnitTestProvider = TestProjectGenerator.UnitTestProvider.SpecRun};
            var projectFile = _appConfigGenerator.Generate(configuration);
            projectFile.Path.Should().Be("app.config");
        }

        [Fact]
        public void BuildActionIsNone()
        {
            var configuration = new Configuration { UnitTestProvider = TestProjectGenerator.UnitTestProvider.SpecRun };
            var projectFile = _appConfigGenerator.Generate(configuration);
            projectFile.BuildAction.Should().Be("None");
        }


        [Fact]
        public void UnitTestProvider()
        {
            var configuration = new Configuration { UnitTestProvider = TestProjectGenerator.UnitTestProvider.SpecRun };
            var projectFile = _appConfigGenerator.Generate(configuration);


            projectFile.Content.Should().Contain("<unitTestProvider name=\"SpecRun\" />");
        }


        [Fact]
        public void SinglePlugin()
        {
            var configuration = new Configuration { UnitTestProvider = TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.Plugins.Add(new SpecFlowPlugin("SpecRun") );
            var projectFile = _appConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain("<plugins>");
            projectFile.Content.Should().Contain("<add name=\"SpecRun\" />");
            projectFile.Content.Should().Contain("</plugins>");
        }

        [Fact]
        public void MultiplePlugins()
        {
            var configuration = new Configuration { UnitTestProvider = TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.Plugins.Add(new SpecFlowPlugin("SpecRun"));
            configuration.Plugins.Add(new SpecFlowPlugin("SpecFlow+Excel"));
            var projectFile = _appConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain("<plugins>");
            projectFile.Content.Should().Contain("<add name=\"SpecRun\" />");
            projectFile.Content.Should().Contain("<add name=\"SpecFlow+Excel\" />");
            projectFile.Content.Should().Contain("</plugins>");
        }

        [Fact]
        public void PluginWithPath()
        {
            var configuration = new Configuration { UnitTestProvider = TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.Plugins.Add(new SpecFlowPlugin("SpecRun", "pathToPluginFolder"));
            var projectFile = _appConfigGenerator.Generate(configuration);
            

            projectFile.Content.Should().Contain("<plugins>");
            projectFile.Content.Should().Contain("<add name=\"SpecRun\" path=\"pathToPluginFolder\" />");
            projectFile.Content.Should().Contain("</plugins>");
        }

        [Fact]
        public void SingleAdditionalStepAssembly()
        {
            var configuration = new Configuration { UnitTestProvider = TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.StepAssemblies.Add(new StepAssembly("AdditionalStepAssembly"));

            var projectFile = _appConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain("<stepAssemblies>");
            projectFile.Content.Should().Contain("<stepAssembly assembly=\"AdditionalStepAssembly\" />");
            projectFile.Content.Should().Contain("</stepAssemblies>");
        }
    }


}
