using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.ConfigurationGenerator
{
    public class AppConfigGenerator : XmlFileGeneratorBase, IConfigurationGenerator
    {
        private readonly ProjectFileFactory _projectFileFactory = new ProjectFileFactory();

        public ProjectFile Generate(Configuration configuration)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = GenerateDefaultXmlWriter(ms))
                {
                    writer.WriteStartElement("configuration");

                    WriteConfigSections(writer, configuration.AppConfigSection);
                    WriteSpecFlow(writer, configuration);

                    writer.WriteEndElement();
                    writer.Flush();

                    return _projectFileFactory.FromStream(ms, "app.config", "None", Encoding.UTF8);
                }
            }
        }

        private void WriteSpecFlow(XmlWriter writer, Configuration configuration)
        {
            writer.WriteStartElement("specFlow");

            WriteUnitTestProvider(writer, configuration.UnitTestProvider.ToName());
            if (configuration.BindingCulture != null)
            {
                WriteBindingCulture(writer, configuration.BindingCulture);
            }

            if (configuration.FeatureLanguage != null)
            {
                WriteLanguage(writer, configuration.FeatureLanguage);
            }

            if (configuration.Generator != null)
            {
                WriteGenerator(writer, configuration.Generator);
            }

            WriteStepAssemblies(writer, configuration.StepAssemblies);
            WritePlugins(writer, configuration.Plugins);

            writer.WriteEndElement();
        }

        private void WriteBindingCulture(XmlWriter writer, CultureInfo bindingCulture)
        {
            writer.WriteStartElement("bindingCulture");
            writer.WriteAttributeString("name", bindingCulture.Name);
            writer.WriteEndElement();
        }

        private void WriteConfigSections(XmlWriter writer, IEnumerable<AppConfigSection> appConfigSections)
        {
            writer.WriteStartElement("configSections");

            foreach (var appConfigSection in appConfigSections)
            {
                WriteConfigSection(writer, appConfigSection);
            }

            writer.WriteEndElement();
        }

        private void WriteConfigSection(XmlWriter writer, AppConfigSection appConfigSection)
        {
            writer.WriteStartElement("section");
            writer.WriteAttributeString("name", appConfigSection.Name);
            writer.WriteAttributeString("type", appConfigSection.Type);
            writer.WriteEndElement();
        }

        private void WriteUnitTestProvider(XmlWriter writer, string unitTestProvider)
        {
            writer.WriteStartElement("unitTestProvider");
            writer.WriteAttributeString("name", unitTestProvider);
            writer.WriteEndElement();
        }

        private void WriteLanguage(XmlWriter writer, CultureInfo featureLanguage)
        {
            writer.WriteStartElement("language");
            writer.WriteAttributeString("feature", featureLanguage.Name);
            writer.WriteEndElement();
        }

        private void WriteStepAssemblies(XmlWriter writer, IEnumerable<StepAssembly> stepAssemblies)
        {
            if (stepAssemblies is null) return;
            writer.WriteStartElement("stepAssemblies");
            foreach (var stepAssembly in stepAssemblies)
            {
                WriteStepAssembly(writer, stepAssembly);
            }

            writer.WriteEndElement();
        }

        private void WriteStepAssembly(XmlWriter writer, StepAssembly stepAssembly)
        {
            writer.WriteStartElement("stepAssembly");
            writer.WriteAttributeString("assembly", stepAssembly.Assembly);
            writer.WriteEndElement();
        }

        private void WritePlugins(XmlWriter writer, IEnumerable<SpecFlowPlugin> plugins)
        {
            if (plugins is null) return;
            writer.WriteStartElement("plugins");
            foreach (var plugin in plugins)
            {
                WritePlugin(writer, plugin);
            }

            writer.WriteEndElement();
        }

        private void WritePlugin(XmlWriter writer, SpecFlowPlugin plugin)
        {
            writer.WriteStartElement("add");
            writer.WriteAttributeString("name", plugin.Name);

            if (!string.IsNullOrEmpty(plugin.Path))
                writer.WriteAttributeString("path", plugin.Path);

            if (plugin.Type != (SpecFlowPluginType.Generator | SpecFlowPluginType.Runtime))
                writer.WriteAttributeString("type", plugin.Type.ToPluginTypeString());

            writer.WriteEndElement();
        }

        private void WriteGenerator(XmlWriter writer, Generator generator)
        {
            writer.WriteStartElement("generator");

            writer.WriteAttributeString("allowDebugGeneratedFiles", generator.AllowDebugGeneratedFiles ? "true" : "false");
            writer.WriteAttributeString("allowRowTests", generator.AllowRowTests ? "true" : "false");
            writer.WriteAttributeString("generateAsyncTests", generator.GenerateAsyncTests? "true" : "false");
            writer.WriteAttributeString("path", generator.Path);

            writer.WriteEndElement();
        }
    }
}
