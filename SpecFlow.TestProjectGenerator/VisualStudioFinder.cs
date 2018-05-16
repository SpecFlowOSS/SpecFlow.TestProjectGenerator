using System;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class VisualStudioFinder
    {
        private const string VsWhereParameter = @"-latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -property installationPath";
        private readonly Folders _folders;
        private readonly IOutputWriter _outputWriter;

        public VisualStudioFinder(Folders folders, IOutputWriter outputWriter)
        {
            _folders = folders;
            _outputWriter = outputWriter;
        }

        public string Find()
        {
            string vsWherePath = Path.Combine(_folders.GlobalPackages, "vswhere", "2.3.2", "tools", "vswhere.exe");

            if (!File.Exists(vsWherePath))
            {
                throw new FileNotFoundException("vswhere can not be found! Is the version number correct?", vsWherePath);
            }

            var ph = new ProcessHelper();
            var processResult = ph.RunProcess(_outputWriter, ".", vsWherePath, VsWhereParameter);


            var lines = processResult.CombinedOutput.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            return lines.First();
        }

        public string FindMSBuild()
        {
            var msbuildExe = Path.Combine(Find(), "MSBuild", "15.0", "Bin", "msbuild.exe");
            return msbuildExe;
        }

        public string FindDevEnv()
        {
            return Path.Combine(Find(), "Common7", "IDE", "devenv.exe");
        }
    }
}