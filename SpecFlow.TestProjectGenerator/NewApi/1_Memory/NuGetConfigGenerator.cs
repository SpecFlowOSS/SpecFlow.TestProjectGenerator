using System.IO;
using System.Text;
using System.Xml;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class NuGetConfigGenerator
    {
        private readonly ProjectFileFactory _projectFileFactory = new ProjectFileFactory();

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

                    return _projectFileFactory.FromStream(ms, "nuget.config", "None");
                }
            }
        }

        private void WriteNuGetSources(XmlWriter writer, NuGetSource[] nuGetSources)
        {
            foreach (var source in nuGetSources ?? new NuGetSource[0])
            {
                WriteNuGetSource(writer, source);
            }
        }

        private void WriteNuGetSource(XmlWriter writer, NuGetSource nuGetSource)
        {
            writer.WriteStartElement("add");
            writer.WriteAttributeString("key", nuGetSource.Key);
            writer.WriteAttributeString("value", nuGetSource.Value);
            writer.WriteEndElement();
        }

        private void WriteNuGetOrgSource(XmlWriter writer)
        {
            var nuGetOrg = new NuGetSource("Nuget.org", "https://api.nuget.org/v3/index.json");

            WriteNuGetSource(writer, nuGetOrg);
        }
    }
}
