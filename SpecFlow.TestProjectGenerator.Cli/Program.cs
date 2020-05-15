using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using TechTalk.SpecFlow.TestProjectGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.Conventions;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;

namespace SpecFlow.TestProjectGenerator.Cli
{
    partial class Program
    {
        private const string DefaultSpecFlowVersion = "3.1.97";
        private const UnitTestProvider DefaultUnitTestProvider = UnitTestProvider.xUnit;
        private const TargetFramework DefaultTargetFramework = TargetFramework.Netcoreapp31;
        private const ProjectFormat DefaultProjectFormat = ProjectFormat.New;
        private const ConfigurationFormat DefaultConfigurationFormat = ConfigurationFormat.Json;

        static int Main(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new Option<DirectoryInfo>(
                    "--out-dir",
                    "The root directory of the code generation output. Default: current directory."),
                new Option<string>(
                    "--sln-name",
                    "The name of the generated solution (directory and sln file). Default: random string."),
                new Option<Version>(
                    "--specflow-version",
                    () => new Version(DefaultSpecFlowVersion),
                    $"The SpecFlow version referenced in the generated project. Default: '{DefaultSpecFlowVersion}'."),
                new Option<UnitTestProvider>(
                    "--unit-test-provider",
                    () => DefaultUnitTestProvider,
                    $"The unit test provider used in the generated project. Default: '{DefaultUnitTestProvider}'."),
                new Option<TargetFramework>(
                    "--target-framework",
                    () => DefaultTargetFramework,
                    $"The target framework of the generated project. Default: '{DefaultTargetFramework}'."),
                new Option<ProjectFormat>(
                    "--project-format",
                    () => DefaultProjectFormat,
                    $"The project format of the generated project file. Default: '{DefaultProjectFormat}'."),
                new Option<ConfigurationFormat>(
                    "--configuration-format",
                    () => DefaultConfigurationFormat,
                    $"The format of the generated SpecFlow configuration file. Default: '{DefaultConfigurationFormat}'.")

            };

            rootCommand.Description = "SpecFlow Test Project Generator";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<DirectoryInfo, string, Version, UnitTestProvider, TargetFramework, ProjectFormat, ConfigurationFormat>(
                (outDir, slnName, specflowVersion, unitTestProvider, targetFramework, projectFormat, configurationFormat) =>
            {
                var services = ConfigureServices();

                services.AddSingleton(s => new SolutionConfiguration
                {
                    OutDir = outDir,
                    SolutionName = slnName
                });

                services.AddSingleton(s => new TestRunConfiguration
                {
                    ProgrammingLanguage = ProgrammingLanguage.CSharp,
                    UnitTestProvider = unitTestProvider,
                    ConfigurationFormat = configurationFormat,
                    ProjectFormat = projectFormat,
                    TargetFramework = targetFramework,
                });

                services.AddSingleton(s => new CurrentVersionDriver
                {
                    SpecFlowVersion = specflowVersion,
                    SpecFlowNuGetVersion = specflowVersion.ToString()
                });

                var serviceProvider = services.BuildServiceProvider();

                SolutionWriteToDiskDriver d = serviceProvider.GetService<SolutionWriteToDiskDriver>();

                //Create test project
                var pd = serviceProvider.GetService<ProjectsDriver>();
                var pb = pd.CreateProject("Proj1", "C#");

                pb.AddFeatureFile(@"
Feature: Simple Feature
	Scenario: Simple Scenario
		Given I use a .NET API
");

                pb.AddStepBinding(@"
    [Given(""I use a .NET API"")]
    public void Do()
    {
        System.DateTime.Now.ToString();
    }
");

                //Remove local NuGet source
                var sd = serviceProvider.GetService<SolutionDriver>();
                sd.NuGetSources.Clear();

                d.WriteSolutionToDisk();
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IOutputWriter, OutputWriter>();
            services.AddSingleton<Folders, FoldersOverride>();
            services.AddSingleton<SolutionNamingConvention, SolutionNamingConventionOverride>();

            services.Scan(scan => scan
                .FromAssemblyOf<SolutionWriteToDiskDriver>()
                    .AddClasses()
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelf()
                        .WithScopedLifetime());

            return services;
        }
    }
}
