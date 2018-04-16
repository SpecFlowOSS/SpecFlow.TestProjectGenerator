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
            featureLanguage = featureLanguage ?? CultureInfo.GetCultureInfo("en-US");

            using (var stringWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    // open root object
                    jsonWriter.WriteStartObject();

                    // open specflow object
                    jsonWriter.WritePropertyName("specFlow");
                    jsonWriter.WriteStartObject();

                    // open unitTestProvider object
                    jsonWriter.WritePropertyName("unitTestProvider");

                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("name");
                    jsonWriter.WriteValue(unitTestProvider);

                    // close unitTestProvider
                    jsonWriter.WriteEndObject();

                    // open language object
                    jsonWriter.WritePropertyName("language");
                    jsonWriter.WriteStartObject();

                    jsonWriter.WritePropertyName("feature");
                    jsonWriter.WriteValue(featureLanguage.Name);

                    // close language object
                    jsonWriter.WriteEndObject();

                    if (!(stepAssemblies is null))
                    {
                        // open stepAssemblies array
                        jsonWriter.WritePropertyName("stepAssemblies");
                        jsonWriter.WriteStartArray();

                        foreach (var stepAssembly in stepAssemblies)
                        {
                            // open stepAssembly object
                            jsonWriter.WriteStartObject();

                            jsonWriter.WritePropertyName("assembly");
                            jsonWriter.WriteValue(stepAssembly.Assembly);

                            // clase stepAssembly object
                            jsonWriter.WriteEndObject();
                        }

                        // close stepAssemblies array
                        jsonWriter.WriteEndArray();
                    }

                    if (!(plugins is null))
                    {
                        // open plugins array
                        jsonWriter.WritePropertyName("plugins");
                        jsonWriter.WriteStartArray();

                        foreach (var plugin in plugins)
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

                        // close plugins array
                        jsonWriter.WriteEndArray();
                    }
                    
                    // close specflow object
                    jsonWriter.WriteEndObject();

                    // close root object
                    jsonWriter.WriteEndObject();
                    jsonWriter.Flush();

                    return new ProjectFile("specflow.json", "None", stringWriter.ToString());
                }
            }
        }
    }
}
