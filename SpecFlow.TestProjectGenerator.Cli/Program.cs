using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using TechTalk.SpecFlow.TestProjectGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.Conventions;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;

namespace SpecFlow.TestProjectGenerator.Cli
{
    partial class Program
    {
        static int Main(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new Option<DirectoryInfo>(
                    "--out-dir",
                    "The root directory for the code generation output. By default the current directory is used."),
                new Option<string>(
                    "--sln-name",
                    "The name of the solution (both the directory and sln file) to be generated. By default the solution name is calculated from a new GUID.")
            };

            rootCommand.Description = "SpecFlow Test Project Generator";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<DirectoryInfo, string>((outDir, slnName) =>
            {
                var services = ConfigureServices();

                services.AddSingleton(s => new SolutionConfiguration
                {
                    OutDir = outDir,
                    SolutionName = slnName
                });

                var serviceProvider = services.BuildServiceProvider();
                SolutionWriteToDiskDriver d = serviceProvider.GetService<SolutionWriteToDiskDriver>();
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
