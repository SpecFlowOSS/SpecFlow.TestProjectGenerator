using System;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class VisualStudioFinder
    {
        private const string VsWhereInstallationPathParameter = @"-latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -property installationPath";
        private const string VsWhereMsBuildParameter = @"-latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe";
        private readonly Folders _folders;
        private readonly IOutputWriter _outputWriter;

        public VisualStudioFinder(Folders folders, IOutputWriter outputWriter)
        {
            _folders = folders;
            _outputWriter = outputWriter;
        }

        public string Find()
        {
            return ExecuteVsWhere(VsWhereInstallationPathParameter);
        }

        public string FindMSBuild()
        {
            return ExecuteVsWhere(VsWhereMsBuildParameter);
        }

        private string ExecuteVsWhere(string vsWhereParameters)
        {
            string vsWherePath = Path.Combine(_folders.GlobalPackages, "vswhere", "2.6.7", "tools", "vswhere.exe");

            if (!File.Exists(vsWherePath))
            {
                throw new FileNotFoundException("vswhere can not be found! Is the version number correct?", vsWherePath);
            }

            var ph = new ProcessHelper();
            var processResult = ph.RunProcess(_outputWriter, ".", vsWherePath, vsWhereParameters);

            var lines = processResult.CombinedOutput.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            return lines.First();
        }
    }
}