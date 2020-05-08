using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class HooksDriver
    {
        private readonly TestProjectFolders _testProjectFolders;

        public HooksDriver(TestProjectFolders testProjectFolders)
        {
            _testProjectFolders = testProjectFolders;
        }

        public void CheckIsHookExecuted(string methodName, int expectedTimesExecuted)
        {
            int hookExecutionCount = GetHookExecutionCount(methodName);
            hookExecutionCount.Should().Be(expectedTimesExecuted, $"{methodName} executed that many times");
        }

        public int GetHookExecutionCount(string methodName)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            string pathToHookLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");
            if (!File.Exists(pathToHookLogFile))
            {
                return 0;
            }

            string content = File.ReadAllText(pathToHookLogFile);
            content.Should().NotBeNull();

            var regex = new Regex($@"-> hook: {methodName}");

            return regex.Matches(content).Count;
        }

        public void CheckIsHookExecutedInOrder(IEnumerable<string> methodNames)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            string pathToHookLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");
            var lines = File.ReadAllLines(pathToHookLogFile);
            var methodNameLines = methodNames.Select(m => $"-> hook: {m}");
            lines.Should().ContainInOrder(methodNameLines);
        }
    }
}
