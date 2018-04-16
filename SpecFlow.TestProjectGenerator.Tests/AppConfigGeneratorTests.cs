using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
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
            var projectFile = _appConfigGenerator.Generate("SpecRun");
            projectFile.Path.Should().Be("app.config");
        }

        [Fact]
        public void BuildActionIsNone()
        {
            var projectFile = _appConfigGenerator.Generate("SpecRun");
            projectFile.BuildAction.Should().Be("None");
        }


        [Fact]
        public void UnitTestProvider()
        {
            var projectFile = _appConfigGenerator.Generate("SpecRun");

            projectFile.Content.Should().Contain("<unitTestProvider name=\"SpecRun\" />");
        }


        [Fact]
        public void SinglePlugin()
        {
            var projectFile = _appConfigGenerator.Generate("SpecRun", plugins: new SpecFlowPlugin[] { new SpecFlowPlugin("SpecRun") });

            projectFile.Content.Should().Contain("<plugins>");
            projectFile.Content.Should().Contain("<add name=\"SpecRun\" />");
            projectFile.Content.Should().Contain("</plugins>");
        }

        [Fact]
        public void MultiplePlugins()
        {
            var projectFile = _appConfigGenerator.Generate("SpecRun", plugins: new SpecFlowPlugin[] { new SpecFlowPlugin("SpecRun"), new SpecFlowPlugin("SpecFlow+Excel") });

            projectFile.Content.Should().Contain("<plugins>");
            projectFile.Content.Should().Contain("<add name=\"SpecRun\" />");
            projectFile.Content.Should().Contain("<add name=\"SpecFlow+Excel\" />");
            projectFile.Content.Should().Contain("</plugins>");
        }

        [Fact]
        public void PluginWithPath()
        {
            var projectFile = _appConfigGenerator.Generate("SpecRun", plugins: new SpecFlowPlugin[] { new SpecFlowPlugin("SpecRun", "pathToPluginFolder") });

            projectFile.Content.Should().Contain("<plugins>");
            projectFile.Content.Should().Contain("<add name=\"SpecRun\" path=\"pathToPluginFolder\" />");
            projectFile.Content.Should().Contain("</plugins>");
        }

        [Fact]
        public void SingleAdditionalStepAssembly()
        {
            var projectFile = _appConfigGenerator.Generate("SpecRun", stepAssemblies: new StepAssembly[] { new StepAssembly("AdditionalStepAssembly") });

            projectFile.Content.Should().Contain("<stepAssemblies>");
            projectFile.Content.Should().Contain("<stepAssembly assembly=\"AdditionalStepAssembly\" />");
            projectFile.Content.Should().Contain("</stepAssemblies>");
        }
    }


}
