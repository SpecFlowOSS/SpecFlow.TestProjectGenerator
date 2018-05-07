using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using SpecFlow.TestProjectGenerator.Helpers;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationModel;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationModel.Dependencies;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator
{
    public class JsonConfigGenerator : IConfigurationGenerator
    {
        public ProjectFile Generate(Configuration configuration)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    // open root object
                    jsonWriter.WriteStartObject();

                    WriteSpecFlow(jsonWriter, configuration);

                    // close root object
                    jsonWriter.WriteEndObject();
                    jsonWriter.Flush();

                    return new ProjectFile("specflow.json", "None", stringWriter.ToString());
                }
            }
        }

        private void WriteSpecFlow(JsonWriter jsonWriter, Configuration configuration)
        {
            configuration.FeatureLanguage = configuration.FeatureLanguage ?? CultureInfo.GetCultureInfo("en-US");

            // open specflow object
            jsonWriter.WritePropertyName("specflow");
            jsonWriter.WriteStartObject();

            WriteUnitTestProvider(jsonWriter, configuration.UnitTestProvider.ToName());

            if (configuration.FeatureLanguage != null)
            {
                WriteLanguage(jsonWriter, configuration.FeatureLanguage);
            }

            if (configuration.BindingCulture != null)
            {
                WriteBindingCulture(jsonWriter, configuration.BindingCulture);
            }

            if (configuration.Generator.IsValueCreated)
            {
                WriteGenerator(jsonWriter, configuration.Generator.Value);
            }

            if (configuration.Runtime.IsValueCreated)
            {
                WriteRuntime(jsonWriter, configuration.Runtime.Value);
            }

            WriteStepAssemblies(jsonWriter, configuration.StepAssemblies);
            WritePlugins(jsonWriter, configuration.Plugins);

            // close specflow object
            jsonWriter.WriteEndObject();
        }

        private void WriteUnitTestProvider(JsonWriter jsonWriter, string unitTestProvider)
        {
            // open unitTestProvider object
            jsonWriter.WritePropertyName("unitTestProvider");

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("name");
            jsonWriter.WriteValue(unitTestProvider);

            // close unitTestProvider
            jsonWriter.WriteEndObject();
        }

        private void WriteLanguage(JsonWriter jsonWriter, CultureInfo featureLanguage)
        {
            // open language object
            jsonWriter.WritePropertyName("language");
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("feature");
            jsonWriter.WriteValue(featureLanguage.Name);

            // close language object
            jsonWriter.WriteEndObject();
        }

        private void WriteBindingCulture(JsonWriter jsonWriter, CultureInfo bindingCulture)
        {
            jsonWriter.WritePropertyName("bindingCulture");
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("name");
            jsonWriter.WriteValue(bindingCulture.Name);
            jsonWriter.WriteEndObject();
        }

        private void WriteStepAssemblies(JsonWriter jsonWriter, IEnumerable<StepAssembly> stepAssemblies)
        {
            if (!(stepAssemblies is null))
            {
                // open stepAssemblies array
                jsonWriter.WritePropertyName("stepAssemblies");
                jsonWriter.WriteStartArray();

                foreach (var stepAssembly in stepAssemblies)
                {
                    WriteStepAssembly(jsonWriter, stepAssembly);
                }

                // close stepAssemblies array
                jsonWriter.WriteEndArray();
            }
        }

        private void WriteStepAssembly(JsonWriter jsonWriter, StepAssembly stepAssembly)
        {
            // open stepAssembly object
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("assembly");
            jsonWriter.WriteValue(stepAssembly.Assembly);

            // clase stepAssembly object
            jsonWriter.WriteEndObject();
        }

        private void WritePlugins(JsonWriter jsonWriter, IEnumerable<SpecFlowPlugin> plugins)
        {
            if (plugins is null) return;

            // open plugins array
            jsonWriter.WritePropertyName("plugins");
            jsonWriter.WriteStartArray();

            foreach (var plugin in plugins)
            {
                WritePlugin(jsonWriter, plugin);
            }

            // close plugins array
            jsonWriter.WriteEndArray();
        }

        private void WritePlugin(JsonWriter jsonWriter, SpecFlowPlugin plugin)
        {
            // open add object
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("name");
            jsonWriter.WriteValue(plugin.Name);

            if (!string.IsNullOrEmpty(plugin.Path))
            {
                jsonWriter.WritePropertyName("path");
                jsonWriter.WriteValue(plugin.Path);
            }

            if (plugin.Type != (SpecFlowPluginType.Generator | SpecFlowPluginType.Runtime))
            {
                jsonWriter.WritePropertyName("type");
                jsonWriter.WriteValue(plugin.Type.ToPluginTypeString());
            }

            // close add object
            jsonWriter.WriteEndObject();
        }

        private void WriteGenerator(JsonWriter jsonWriter, Generator generator)
        {
            jsonWriter.WritePropertyName("generator");
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("allowDebugGeneratedFiles");
            jsonWriter.WriteValue(generator.AllowDebugGeneratedFiles);

            jsonWriter.WritePropertyName("allowRowTests");
            jsonWriter.WriteValue(generator.AllowRowTests);

            jsonWriter.WritePropertyName("generateAsyncTests");
            jsonWriter.WriteValue(generator.GenerateAsyncTests);

            jsonWriter.WritePropertyName("path");
            jsonWriter.WriteValue(generator.Path);

            if (generator.Dependencies.Count > 0)
            {
                jsonWriter.WritePropertyName("dependencies");
                WriteDependencies(jsonWriter, generator.Dependencies);
            }

            jsonWriter.WriteEndObject();
        }

        private void WriteRuntime(JsonWriter jsonWriter, Runtime runtime)
        {
            jsonWriter.WritePropertyName("runtime");
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("missingOrPendingStepsOutcome");
            jsonWriter.WriteValue(runtime.MissingOrPendingStepsOutcome.ToString());

            jsonWriter.WritePropertyName("stopAtFirstError");
            jsonWriter.WriteValue(runtime.StopAtFirstError);

            if (runtime.Dependencies.Count > 0)
            {
                jsonWriter.WritePropertyName("dependencies");
                WriteDependencies(jsonWriter, runtime.Dependencies);
            }

            jsonWriter.WriteEndObject();
        }

        private void WriteDependencies(JsonWriter jsonWriter, IEnumerable<IDependency> dependencies)
        {
            jsonWriter.WriteStartArray();

            foreach (var dependency in dependencies)
            {
                switch (dependency)
                {
                    case RegisterDependency registerDependency:
                        WriteRegisterDependency(jsonWriter, registerDependency);
                        break;
                    case null: throw new InvalidOperationException("null is not supported as dependency.");
                    default: throw new NotSupportedException($"Dependency type {dependency.GetType()} is not supported.");
                }
            }

            jsonWriter.WriteEndArray();
        }

        private void WriteRegisterDependency(JsonWriter jsonWriter, RegisterDependency dependency)
        {
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("type");
            jsonWriter.WriteValue(dependency.Type);

            jsonWriter.WritePropertyName("as");
            jsonWriter.WriteValue(dependency.As);

            if (dependency.Name.IsNotNullOrWhiteSpace())
            {
                jsonWriter.WritePropertyName("name");
                jsonWriter.WriteValue(dependency.Name);
            }

            jsonWriter.WriteEndObject();
        }
    }
}
