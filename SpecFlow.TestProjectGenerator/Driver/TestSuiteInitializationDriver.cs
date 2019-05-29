using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class TestSuiteInitializationDriver
    {
        public DateTime? OverrideTestSuiteStartupTime { get; set; }

        public Guid? OverridePickleId { get; set; }

        public DateTime? OverrideTestCaseStartedTime { get; set; }
    }
}
