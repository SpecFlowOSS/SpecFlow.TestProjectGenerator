using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class NuGetConfigGenerator
    {
        public ProjectFile Generate(NuGetSource[] nuGetSources = null)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(ms, Encoding.UTF8))
                {
                    writer.WriteStartElement("configuration");

                    writer.WriteStartElement("packageSources");

                    foreach (var source in nuGetSources ?? new NuGetSource[0])
                    {
                        writer.WriteStartElement("add");
                        writer.WriteAttributeString("key", source.Key);
                        writer.WriteAttributeString("value", source.Value);
                        writer.WriteEndElement();
                    }

                    // <add key="Nuget.org" value="https://api.nuget.org/v3/index.json" />
                    writer.WriteStartElement("add");
                    writer.WriteAttributeString("key", "Nuget.org");
                    writer.WriteAttributeString("value", "https://api.nuget.org/v3/index.json");
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.Flush();

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var sr = new StreamReader(ms))
                    {
                        return new ProjectFile("nuget.config", "None", sr.ReadToEnd());
                    }
                }
            }
        }
    }
}
