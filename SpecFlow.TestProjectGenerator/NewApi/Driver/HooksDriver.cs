using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._5_TestRun;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi.Driver
{
    public class HooksDriver
    {
        private readonly VSTestExecutionDriver _vsTestExecutionDriver;

        public HooksDriver(VSTestExecutionDriver vsTestExecutionDriver)
        {
            _vsTestExecutionDriver = vsTestExecutionDriver;
        }

        public void CheckIsHookExecuted(string methodName, int times)
        {
            _vsTestExecutionDriver.LastTestExecutionResult.Should().NotBeNull();
            var lines = GetLines(_vsTestExecutionDriver.LastTestExecutionResult.Output);
            lines.Where(l => l == $"-> hook: {methodName}")
                 .Should()
                 .HaveCount(times);
        }

        public void CheckIsHookExecutedInOrder(IEnumerable<string> methodNames)
        {
            _vsTestExecutionDriver.LastTestExecutionResult.Should().NotBeNull();
            var lines = GetLines(_vsTestExecutionDriver.LastTestExecutionResult.Output);
            var methodNameLines = methodNames.Select(m => $"-> hook: {m}");
            lines.Should().ContainInOrder(methodNameLines);
        }

        private IEnumerable<string> GetLines(string value)
        {
            var lines = new List<string>();
            using (var sr = new StringReader(value))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }
    }
}
