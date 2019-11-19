using FluentAssertions;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class CompilationDriver
    {
        private const BuildTool DefaultBuildTool = BuildTool.DotnetBuild;

        private readonly Compiler _compiler;
        private readonly SolutionWriteToDiskDriver _solutionWriteToDiskDriver;
        private CompileResult _compileResult;

        public CompilationDriver(Compiler compiler, SolutionWriteToDiskDriver solutionWriteToDiskDriver)
        {
            _compiler = compiler;
            _solutionWriteToDiskDriver = solutionWriteToDiskDriver;
        }

        public bool HasTriedToCompile { get; private set; }

        public void CompileSolution(BuildTool buildTool = DefaultBuildTool, bool? treatWarningsAsErrors = null)
        {
            CompileSolutionTimes(1, buildTool, treatWarningsAsErrors);
        }

        public void CompileSolutionTimes(uint times, BuildTool buildTool = DefaultBuildTool, bool? treatWarningsAsErrors = null)
        {
            HasTriedToCompile = true;
            _solutionWriteToDiskDriver.WriteSolutionToDisk(treatWarningsAsErrors);

            for (uint time = 0; time < times; time++)
            {
                _compileResult = _compiler.Run(buildTool, treatWarningsAsErrors);
            }
        }

        public void CheckSolutionShouldHaveCompiled()
        {
            _compileResult.Should().NotBeNull("the project should have compiled");
            _compileResult.IsSuccessful.Should().BeTrue("the project should have compiled successfully.\r\n\r\n------ Build output ------\r\n{0}", _compileResult.Output);
        }

        public void CheckSolutionShouldHaveCompileError()
        {
            _compileResult.Should().NotBeNull("the project should have compiled");
            _compileResult.IsSuccessful.Should().BeFalse("There should be a compile error");
        }

        public bool CheckCompileOutputForString(string str)
        {
            return _compileResult.Output.Contains(str);
        }
    }
}
