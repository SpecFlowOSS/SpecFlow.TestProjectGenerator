using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class JsonConfigGenerator
    {
        public ProjectFile Generate(string unitTestProvider = null, StepAssembly[] stepAssemblies = null, SpecFlowPlugin[] plugins = null, CultureInfo featureLanguage = null)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    // open root object
                    jsonWriter.WriteStartObject();

                    WriteSpecFlow(jsonWriter, unitTestProvider, stepAssemblies, plugins, featureLanguage);

                    // close root object
                    jsonWriter.WriteEndObject();
                    jsonWriter.Flush();

                    return new ProjectFile("specflow.json", "None", stringWriter.ToString());
                }
            }
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

        private void WriteStepAssemblies(JsonWriter jsonWriter, StepAssembly[] stepAssemblies)
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

        private void WritePlugins(JsonWriter jsonWriter, SpecFlowPlugin[] plugins)
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

        private void WriteSpecFlow(JsonWriter jsonWriter, string unitTestProvider = null, StepAssembly[] stepAssemblies = null, SpecFlowPlugin[] plugins = null, CultureInfo featureLanguage = null)
        {
            featureLanguage = featureLanguage ?? CultureInfo.GetCultureInfo("en-US");
            unitTestProvider = unitTestProvider ?? "MsTest";

            // open specflow object
            jsonWriter.WritePropertyName("specFlow");
            jsonWriter.WriteStartObject();

            WriteUnitTestProvider(jsonWriter, unitTestProvider);

            WriteLanguage(jsonWriter, featureLanguage);

            WriteStepAssemblies(jsonWriter, stepAssemblies);

            WritePlugins(jsonWriter, plugins);

            // close specflow object
            jsonWriter.WriteEndObject();
        }
    }
}
