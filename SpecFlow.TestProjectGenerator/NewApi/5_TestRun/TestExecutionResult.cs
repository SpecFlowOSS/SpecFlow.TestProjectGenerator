using System.Collections.Generic;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._5_TestRun
{
    public class TestExecutionResult
    {
        public int Total { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public int Pending { get; set; }
        public int Ignored { get; set; }
        public string Output { get; set; }
        public string TrxOutput { get; set; }
        public int Executed { get; set; }

        public List<TestResult> TestResults { get; set; }
        public List<string> ReportFiles { get; set; }
        public int Warning { get; set; }
        public string LogFileContent { get; set; }
    }

    public class TestResult
    {
        public string Id { get; set; }
        public string Outcome { get; set; }
        public string StdOut { get; set; }
    }
}