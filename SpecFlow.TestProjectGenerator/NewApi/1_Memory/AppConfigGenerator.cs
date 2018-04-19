using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class AppConfigGenerator
    {
        public ProjectFile Generate(string unitTestProvider, StepAssembly[] stepAssemblies = null, SpecFlowPlugin[] plugins = null, CultureInfo featureLanguage = null)
        {
            featureLanguage = featureLanguage ?? CultureInfo.GetCultureInfo("en-US");

            using (var ms = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(ms, Encoding.UTF8))
                {
                    writer.WriteStartElement("configuration");

                    WriteSpecFlow(writer, unitTestProvider, stepAssemblies, plugins, featureLanguage);

                    writer.WriteEndElement();
                    writer.Flush();

                    return GenerateProjectFile(ms);
                }
            }
        }

        public void WriteSpecFlow(XmlWriter writer, string unitTestProvider, StepAssembly[] stepAssemblies = null, SpecFlowPlugin[] plugins = null, CultureInfo featureLanguage = null)
        {
            writer.WriteStartElement("specFlow");

            WriteUnitTestProvider(writer, unitTestProvider);

            WriteLanguage(writer, featureLanguage);

            WriteStepAssemblies(writer, stepAssemblies);

            WritePlugins(writer, plugins);

            writer.WriteEndElement();
        }

        public void WriteUnitTestProvider(XmlWriter writer, string unitTestProvider)
        {
            writer.WriteStartElement("unitTestProvider");
            writer.WriteAttributeString("name", unitTestProvider);
            writer.WriteEndElement();
        }

        public void WriteLanguage(XmlWriter writer, CultureInfo featureLanguage)
        {
            writer.WriteStartElement("language");
            writer.WriteAttributeString("feature", featureLanguage.Name);
            writer.WriteEndElement();
        }

        public void WriteStepAssemblies(XmlWriter writer, StepAssembly[] stepAssemblies)
        {
            if (stepAssemblies is null) return;
            writer.WriteStartElement("stepAssemblies");
            foreach (var stepAssembly in stepAssemblies)
            {
                WriteStepAssembly(writer, stepAssembly);
            }

            writer.WriteEndElement();

        }

        public void WriteStepAssembly(XmlWriter writer, StepAssembly stepAssembly)
        {
            writer.WriteStartElement("stepAssembly");
            writer.WriteAttributeString("assembly", stepAssembly.Assembly);
            writer.WriteEndElement();
        }

        public void WritePlugins(XmlWriter writer, SpecFlowPlugin[] plugins)
        {
            if (plugins is null) return;
            writer.WriteStartElement("plugins");
            foreach (var plugin in plugins)
            {
                WritePlugin(writer, plugin);
            }

            writer.WriteEndElement();
        }

        public void WritePlugin(XmlWriter writer, SpecFlowPlugin plugin)
        {
            writer.WriteStartElement("add");
            writer.WriteAttributeString("name", plugin.Name);

            if (!string.IsNullOrEmpty(plugin.Path))
                writer.WriteAttributeString("path", plugin.Path);

            if (plugin.Type != (SpecFlowPluginType.Generator | SpecFlowPluginType.Runtime))
                writer.WriteAttributeString("type", plugin.Type.ToPluginTypeString());


            writer.WriteEndElement();
        }

        public ProjectFile GenerateProjectFile(MemoryStream ms)
        {
            ms.Seek(0, SeekOrigin.Begin);

            using (var sr = new StreamReader(ms))
            {
                return new ProjectFile("app.config", "None", sr.ReadToEnd());
            }
        }
    }
}
