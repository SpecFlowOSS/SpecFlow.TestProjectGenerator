using System;
using System.Text.RegularExpressions;

namespace SpecFlow.TestProjectGenerator.NewApi._4_Compile
{
    public class CompileResult
    {
        public CompileResult(int exitCode, string output)
        {
            ExitCode = exitCode;
            IsSuccessful = exitCode == 0;
            Output = output;
        }

        public int ExitCode { get; }
        public bool IsSuccessful { get; }
        public string Output { get; }
    }
}