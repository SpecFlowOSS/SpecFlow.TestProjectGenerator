using System;
using System.Linq;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class CurrentVersionDriver
    {
        public CurrentVersionDriver()
        {
            var specFlowAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "TechTalk.SpecFlow").SingleOrDefault();
            if (specFlowAssembly != null)
            {
                var specFlowVersion = specFlowAssembly.GetName().Version;

                

                SpecFlowMajor = specFlowVersion.Major;
                SpecFlowMinor = specFlowVersion.Minor;

                SpecFlowVersion = $"{specFlowVersion.Major}.{specFlowVersion.Minor}.0";
                SpecFlowVersionDash = $"{specFlowVersion.Major}-{specFlowVersion.Minor}-0";
            }
        }

        public string SpecFlowVersionDash { get; private set; }

        public string SpecFlowVersion { get; private set; }
        public int SpecFlowMajor { get; set; }
        public int SpecFlowMinor { get; set; }
        public string SpecFlowNuGetVersion { get; set; }
        public string NuGetVersion { get; set; }
        public string MajorMinorPatchVersion { get; set; }
    }
}
