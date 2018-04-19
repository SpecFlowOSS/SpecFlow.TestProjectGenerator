using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class NuGetPackageAssembly
    {
        public NuGetPackageAssembly(string publicAssemblyName, string relativeHintPath)
        {
            PublicAssemblyName = publicAssemblyName;
            RelativeHintPath = relativeHintPath;
        }

        public string PublicAssemblyName { get; }
        public string RelativeHintPath { get; }
    }
}
