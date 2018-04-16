using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class PackagesConfigGenerator
    {

        public ProjectFile Generate(IEnumerable<NuGetPackage> nuGetPackages, TargetFramework targetFramework)
        {
            using (var ms = new MemoryStream())
            {
                using (var xw = new XmlTextWriter(ms, Encoding.UTF8))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("packages");

                    string tfm = targetFramework == 0 ? null : targetFramework.ToTargetFrameworkMoniker();

                    foreach (var package in nuGetPackages)
                    {
                        xw.WriteStartElement("package");
                        xw.WriteAttributeString("id", package.Name);
                        xw.WriteAttributeString("version", package.Version);

                        if (!(tfm is null))
                        {
                            xw.WriteAttributeString("targetFramework", tfm);
                        }

                        xw.WriteEndElement();
                    }

                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                    xw.Flush();

                    ms.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(ms))
                    {
                        return new ProjectFile("packages.config", "None", sr.ReadToEnd());
                    }
                }
            }
        }
    }
}
