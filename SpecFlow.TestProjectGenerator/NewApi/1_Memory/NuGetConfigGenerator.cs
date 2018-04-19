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

                    WriteNuGetSources(writer, nuGetSources);

                    WriteNuGetOrgSource(writer);

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.Flush();

                    return GenerateProjectFile(ms);

                }
            }
        }

        public void WriteNuGetSources(XmlWriter writer, NuGetSource[] nuGetSources)
        {
            foreach (var source in nuGetSources ?? new NuGetSource[0])
            {
                WriteNuGetSource(writer, source);
            }
        }

        public void WriteNuGetSource(XmlWriter writer, NuGetSource nuGetSource)
        {
            writer.WriteStartElement("add");
            writer.WriteAttributeString("key", nuGetSource.Key);
            writer.WriteAttributeString("value", nuGetSource.Value);
            writer.WriteEndElement();
        }

        public void WriteNuGetOrgSource(XmlWriter writer)
        {
            var nuGetOrg = new NuGetSource("Nuget.org", "https://api.nuget.org/v3/index.json");

            WriteNuGetSource(writer, nuGetOrg);
        }

        public ProjectFile GenerateProjectFile(MemoryStream ms)
        {
            ms.Seek(0, SeekOrigin.Begin);

            using (var sr = new StreamReader(ms))
            {
                return new ProjectFile("nuget.config", "None", sr.ReadToEnd());
            }
        }
    }
}
