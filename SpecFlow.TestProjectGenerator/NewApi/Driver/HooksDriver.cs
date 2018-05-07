using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;

namespace SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class HooksDriver
    {
        private readonly TestProjectFolders _testProjectFolders;

        public HooksDriver(TestProjectFolders testProjectFolders)
        {
            _testProjectFolders = testProjectFolders;
        }

        public string GetHookLogStatement(string methodName)
        {
            string pathToLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "hooks.log");
            return $@"System.IO.File.AppendAllText(@""{pathToLogFile}"", ""-> hook: {methodName}"");";
        }

        public void CheckIsHookExecuted(string methodName, int times)
        {
            string pathToLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "hooks.log");
            var lines = File.ReadAllLines(pathToLogFile);

            lines.Where(l => l == $"-> hook: {methodName}")
                 .Should()
                 .HaveCount(times);
        }

        public void CheckIsHookExecutedInOrder(IEnumerable<string> methodNames)
        {
            string pathToLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "hooks.log");
            var lines = File.ReadAllLines(pathToLogFile);
            var methodNameLines = methodNames.Select(m => $"-> hook: {m}");

            lines.Should().ContainInOrder(methodNameLines);
        }
    }
}
