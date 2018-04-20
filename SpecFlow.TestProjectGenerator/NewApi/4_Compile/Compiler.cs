using System;

namespace SpecFlow.TestProjectGenerator.NewApi._4_Compile
{
    public class Compiler
    {
        private readonly VisualStudioFinder _visualStudioFinder;
        private readonly TestProjectFolders _testProjectFolders;

        public Compiler(VisualStudioFinder visualStudioFinder, TestProjectFolders testProjectFolders)
        {
            _visualStudioFinder = visualStudioFinder;
            _testProjectFolders = testProjectFolders;
        }

        public CompileResult Run()
        {
            string msBuildPath = _visualStudioFinder.FindMSBuild();
            Console.WriteLine("Invoke MsBuild from {0}", msBuildPath);

            var processHelper = new ProcessHelper();
            var msBuildProcess = processHelper.RunProcess(_testProjectFolders.PathToSolutionDirectory, msBuildPath, $"/bl /nologo /v:m \"{_testProjectFolders.PathToSolutionFile}\"");

            return new CompileResult(successful: msBuildProcess.ExitCode == 0, output: msBuildProcess.CombinedOutput);

            //if (msBuildProcess.ExitCode > 0)
            //{
            //    var firstErrorLine = msBuildProcess.CombinedOutput.SplitByString(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(l => l.Contains("): error "));
            //    throw new Exception($"Build failed: {firstErrorLine}");
            //}
        }
    }
}
