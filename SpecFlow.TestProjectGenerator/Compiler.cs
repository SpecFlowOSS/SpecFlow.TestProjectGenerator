namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class Compiler
    {
        private readonly VisualStudioFinder _visualStudioFinder;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly IOutputWriter _outputWriter;

        public Compiler(VisualStudioFinder visualStudioFinder, TestProjectFolders testProjectFolders, IOutputWriter outputWriter)
        {
            _visualStudioFinder = visualStudioFinder;
            _testProjectFolders = testProjectFolders;
            _outputWriter = outputWriter;
        }

        public CompileResult Run()
        {
            string msBuildPath = _visualStudioFinder.FindMSBuild();
            _outputWriter.WriteLine($"Invoke MsBuild from {msBuildPath}");

            var processHelper = new ProcessHelper();
            var msBuildProcess = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, msBuildPath, $"/bl /nologo /v:m \"{_testProjectFolders.PathToSolutionFile}\"");

            if (msBuildProcess.ExitCode == 0)
            {
                
            }

            return new CompileResult(msBuildProcess.ExitCode, msBuildProcess.CombinedOutput);
        }
    }
}
