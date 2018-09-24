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

        public void CheckIsHookExecuted(string methodName, int timesExecuted)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            string pathToHookLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");
            string content = File.ReadAllText(pathToHookLogFile);
            content.Should().NotBeNull();

            var regex = new Regex($@"-> hook: {methodName}");

            regex.Matches(content).Count.Should().Be(timesExecuted);
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
