using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using Xunit;

namespace SpecFlow.TestProjectGenerator.Tests
{
    public class JsonConfigGeneratorTests
    {
        private readonly JsonConfigGenerator _JsonConfigGenerator;

        public JsonConfigGeneratorTests()
        {
            _JsonConfigGenerator = new JsonConfigGenerator();
        }

        [Fact]
        public void UnitTestProvider()
        {
            var projectFile = _JsonConfigGenerator.Generate("SpecRun");

            projectFile.Content.Should().Contain("\"unitTestProvider\":{ \"name\"=\"SpecRun\"} />");
        }


        [Fact]
        public void SinglePlugin()
        {
            var projectFile = _JsonConfigGenerator.Generate("SpecRun", plugins: new SpecFlowPlugin[] { new SpecFlowPlugin("SpecRun") });

            projectFile.Content.Should().Contain(@"{ ""name"": ""SpecRun"" }");
        }

        [Fact]
        public void MultiplePlugins()
        {
            var projectFile = _JsonConfigGenerator.Generate("SpecRun", plugins: new SpecFlowPlugin[] { new SpecFlowPlugin("SpecRun"), new SpecFlowPlugin("SpecFlow+Excel") });

            projectFile.Content.Should().Contain(@"{ ""name"": ""SpecRun"" }");
            projectFile.Content.Should().Contain(@"{ ""name"": ""SpecFlow+Excel"" }");
        }

        [Fact]
        public void PluginWithPath()
        {
            var projectFile = _JsonConfigGenerator.Generate("SpecRun", plugins: new SpecFlowPlugin[] { new SpecFlowPlugin("SpecRun", "pathToPluginFolder") });

            projectFile.Content.Should().Contain(@"{ ""name"": ""SpecRun"", ""path"": ""pathToPluginFolder"" }");
        }

        [Fact]
        public void SingleAdditionalStepAssembly()
        {
            var projectFile = _JsonConfigGenerator.Generate("SpecRun", stepAssemblies: new StepAssembly[] { new StepAssembly("AdditionalStepAssembly") });

            projectFile.Content.Should().Contain(@"[ {""assembly"": ""AdditionalStepAssembly""} ]/>");
            
        }
    }


}
