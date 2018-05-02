using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class AppConfigGenerator : XmlFileGeneratorBase
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
                    WriteSpecFlow(writer, configuration.UnitTestProvider.ToName(), configuration.StepAssemblies, configuration.Plugins, configuration.FeatureLanguage, configuration.BindingCulture);

                    writer.WriteEndElement();
                    writer.Flush();

                    return _projectFileFactory.FromStream(ms, "app.config", "None", Encoding.UTF8);
                }
            }
        }

        private void WriteSpecFlow(XmlWriter writer, string unitTestProvider, IEnumerable<StepAssembly> stepAssemblies, IEnumerable<SpecFlowPlugin> plugins, CultureInfo featureLanguage, CultureInfo bindingCulture)
        {
            writer.WriteStartElement("specFlow");

            WriteUnitTestProvider(writer, unitTestProvider);
            if (bindingCulture != null)
            {
                WriteBindingCulture(writer, bindingCulture);
            }
            if (featureLanguage != null)
            {
                WriteLanguage(writer, featureLanguage);
            }
            WriteStepAssemblies(writer, stepAssemblies);
            WritePlugins(writer, plugins);

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
    }
}
