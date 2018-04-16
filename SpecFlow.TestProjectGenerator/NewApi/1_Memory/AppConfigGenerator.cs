using System;
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

                    writer.WriteStartElement("specFlow");

                    writer.WriteStartElement("unitTestProvider");
                    writer.WriteAttributeString("name", unitTestProvider);
                    writer.WriteEndElement();

                    writer.WriteStartElement("language");
                    writer.WriteAttributeString("feature", featureLanguage.Name);
                    writer.WriteEndElement();

                    if (!(stepAssemblies is null))
                    {
                        writer.WriteStartElement("stepAssemblies");
                        foreach (var stepAssembly in stepAssemblies)
                        {
                            writer.WriteStartElement("stepAssembly");
                            writer.WriteAttributeString("assembly", stepAssembly.Assembly);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    if (!(plugins is null))
                    {
                        writer.WriteStartElement("plugins");
                        foreach (var plugin in plugins)
                        {
                            writer.WriteStartElement("add");
                            writer.WriteAttributeString("name", plugin.Name);

                            if(!string.IsNullOrEmpty(plugin.Path))
                            writer.WriteAttributeString("path", plugin.Path);

                            if(plugin.Type != (SpecFlowPluginType.Generator | SpecFlowPluginType.Runtime))
                            writer.WriteAttributeString("type", plugin.Type.ToPluginTypeString());


                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.Flush();

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var sr = new StreamReader(ms))
                    {
                        return new ProjectFile("app.config", "None", sr.ReadToEnd());
                    }
                }
            }
        }
    }
}