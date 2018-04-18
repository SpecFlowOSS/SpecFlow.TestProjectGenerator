using System.Collections.Generic;

namespace SpecFlow.TestProjectGenerator.NewApi._5_TestRun
{
    internal class TestExecutionResult
    {
        public int Total { get; set; }
        public int TotalSucceeded { get; set; }
        public int TotalFailure { get; set; }
        public int TotalPending { get; set; }
        public int TotalIgnored { get; set; }
        public string Output { get; set; }
    }
}