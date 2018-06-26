using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentAssertions;
using TechTalk.SpecFlow.TestProjectGenerator.Helpers;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._5_TestRun
{
    public class VSTestExecutionDriver
    {
        private readonly VisualStudioFinder _visualStudioFinder;
        private readonly AppConfigDriver _appConfigDriver;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly IOutputWriter _outputWriter;
        private readonly TestRunConfiguration _testRunConfiguration;

        private const string BeginnOfTrxFileLine = "Results File: ";
        private const string BeginnOfLogFileLine = "Log file: ";

        public VSTestExecutionDriver(VisualStudioFinder visualStudioFinder, AppConfigDriver appConfigDriver, TestProjectFolders testProjectFolders, IOutputWriter outputWriter, TestRunConfiguration testRunConfiguration)
        {
            _visualStudioFinder = visualStudioFinder;
            _appConfigDriver = appConfigDriver;
            _testProjectFolders = testProjectFolders;
            _outputWriter = outputWriter;
            _testRunConfiguration = testRunConfiguration;
        }

        public TestExecutionResult LastTestExecutionResult { get; private set; }
        public string RunSettingsFile { get; set; }
        public string Filter { get; set; }

        public void CheckIsBindingMethodExecuted(string methodName, int timesExecuted)
        {
            string pathToLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");
            string logFileContent = File.ReadAllText(pathToLogFile);

            var regex = new Regex($@"-> step: {methodName}");

            regex.Match(logFileContent).Success.Should().BeTrue($"method {methodName} was not executed.");
            regex.Matches(logFileContent).Count.Should().Be(timesExecuted);
        }

        public void CheckOutputContainsText(string text)
        {
            LastTestExecutionResult.Output.Should().NotBeNull()
                                   .And.Subject.Should().Contain(text);
        }

        public void CheckAnyOutputContainsText(string text)
        {
            bool trxContainsEntry = LastTestExecutionResult.TrxOutput.Contains(text);
            bool outputContainsEntry = LastTestExecutionResult.Output.Contains(text);
            bool containsAtAll = trxContainsEntry || outputContainsEntry;
            containsAtAll.Should().BeTrue($"either Trx output or program output should contain '{text}'");
        }

        public TestExecutionResult ExecuteTests()
        {
            string vsFolder = _visualStudioFinder.Find();
            vsFolder = Path.Combine(vsFolder, _appConfigDriver.VSTestPath);

            var vsTestConsoleExePath = Path.Combine(AssemblyFolderHelper.GetTestAssemblyFolder(), Environment.ExpandEnvironmentVariables(vsFolder + @"\vstest.console.exe"));

            var processHelper = new ProcessHelper();
            string arguments = GenereateVsTestsArguments();
            ProcessResult processResult;
            try
            {
                processResult = processHelper.RunProcess(_outputWriter, _testProjectFolders.ProjectFolder, vsTestConsoleExePath, arguments);
            }
            catch (Exception)
            {
                Console.WriteLine($"running vstest.console.exe failed - {_testProjectFolders.CompiledAssemblyPath} {vsTestConsoleExePath} {arguments}");
                throw;
            }

            string output = processResult.CombinedOutput;

            var lines = output.SplitByString(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var trxFiles = FindFilePath(lines, ".trx", BeginnOfTrxFileLine);
            var logFiles = FindFilePath(lines, ".log", BeginnOfLogFileLine);

            if (trxFiles.Count() != 1)
                throw new Exception("No or to many trx files in output found!" + Environment.NewLine + string.Join(Environment.NewLine, trxFiles));

            string logFileContent = String.Empty;
            if (logFiles.Count() == 1)
            {
                logFileContent = File.ReadAllText(new Uri(logFiles.Single().Substring(BeginnOfLogFileLine.Length)).LocalPath);
            }

            var reportFiles = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Where(i => i.StartsWith("Report file:")).Select(i => i.Substring("Report file: ".Length)).Select(i => new Uri(i).AbsolutePath).ToList();

            var trxFile = trxFiles.Single().Substring(BeginnOfTrxFileLine.Length);
            var testResultDocument = XDocument.Load(trxFile);

            var executionResult = new TestExecutionResult()
            {
                ValidLicense = false //not possible to check license with VSTest execution
            };

            XmlNameTable nameTable = new NameTable();
            var namespaceManager = new XmlNamespaceManager(nameTable);
            namespaceManager.AddNamespace("mstest", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            var unitTestExecutionResults = testResultDocument.XPathSelectElements("mstest:TestRun/mstest:Results/mstest:UnitTestResult/mstest:Output/mstest:StdOut", namespaceManager);
            var summaryElement = testResultDocument.XPathSelectElement("//mstest:ResultSummary/mstest:Counters", namespaceManager);
            if (summaryElement != null)
            {
                executionResult.Total = int.Parse(summaryElement.Attribute("total").Value);
                executionResult.Executed = int.Parse(summaryElement.Attribute("executed").Value);
                executionResult.Succeeded = int.Parse(summaryElement.Attribute("passed").Value);
                executionResult.Ignored = GetIgnoredCount(_testRunConfiguration, testResultDocument, executionResult, namespaceManager);
                executionResult.Pending = GetPendingCount(_testRunConfiguration, testResultDocument, executionResult, output, summaryElement, namespaceManager);
                executionResult.Failed = GetFailedCount(_testRunConfiguration, summaryElement, executionResult);
                executionResult.Output = output;
                executionResult.TrxOutput = unitTestExecutionResults.Aggregate(new StringBuilder(), (acc, c) => acc.AppendLine(c.Value)).ToString();
                executionResult.TestResults = testResultDocument.XPathSelectElements("//mstest:Results/mstest:UnitTestResult", namespaceManager).Select(e => new TestResult()
                {
                    Id = e.Attribute("executionId").Value,
                    Outcome = e.Attribute("outcome").Value,
                    StdOut = e.XPathSelectElement("//mstest:Output/mstest:StdOut", namespaceManager).Value
                }).ToList();
                executionResult.ReportFiles = reportFiles;
                executionResult.LogFileContent = logFileContent;
            }

            LastTestExecutionResult = executionResult;

            return executionResult;
        }

        private int GetIgnoredCount(TestRunConfiguration testRunConfiguration, XDocument testResultDocument, TestExecutionResult executionResult, XmlNamespaceManager namespaceManager)
        {
            switch (testRunConfiguration.UnitTestProvider)
            {
                case UnitTestProvider.NUnit3:
                    var unitTestResultElements = testResultDocument.XPathSelectElements("//mstest:Results/mstest:UnitTestResult", namespaceManager);
                    
                    var elements = from resultElement in unitTestResultElements
                                   where resultElement.Attribute("outcome")?.Value == "NotExecuted"
                                   let messageElement = resultElement.XPathSelectElement("//mstest:Output/mstest:ErrorInfo/mstest:Message", namespaceManager)
                                   where messageElement?.Value.Contains("Ignored scenario") == true
                                         || messageElement?.Value.Contains("Ignored feature") == true
                                   select resultElement;

                    return elements.Count();
                default: return executionResult.Total - executionResult.Executed;
            }
            
        }

        private int GetFailedCount(TestRunConfiguration testRunConfiguration, XElement summaryElement, TestExecutionResult executionResult)
        {
            switch (testRunConfiguration.UnitTestProvider)
            {
                case UnitTestProvider.MSTest:
                case UnitTestProvider.XUnit:
                    return int.Parse(summaryElement.Attribute("failed").Value) - executionResult.Pending;
                default:
                    return int.Parse(summaryElement.Attribute("failed").Value);
            }
        }

        private int GetPendingCount(TestRunConfiguration testRunConfiguration, XDocument testResultDocument, TestExecutionResult executionResult, string output, XElement summaryElement, XmlNamespaceManager namespaceManager)
        {
            switch (testRunConfiguration.UnitTestProvider)
            {
                case UnitTestProvider.MSTest:
                    var unitTestResultMessageElements = testResultDocument.XPathSelectElements("//mstest:Results/mstest:UnitTestResult/mstest:Output/mstest:ErrorInfo/mstest:Message", namespaceManager);

                    return unitTestResultMessageElements.Where(e => e.Value.Contains("Microsoft.VisualStudio.TestTools.UnitTesting.AssertInconclusiveException")).Count();

                case UnitTestProvider.XUnit:
                    return GetXUnitPendingCount(output);
                case UnitTestProvider.NUnit3:
                    return executionResult.Total - executionResult.Executed - executionResult.Ignored;
            }

            return int.Parse(summaryElement.Attribute("inconclusive").Value);
        }



        private int GetXUnitPendingCount(string output)
        {
            return Regex.Matches(output, "XUnitPendingStepException").Count / 2 +
                   Regex.Matches(output, "XUnitInconclusiveException").Count / 2;
        }

        private IEnumerable<string> FindFilePath(string[] lines, string ending, string starting)
        {
            return from l in lines
                   let trimmed = l.Trim()
                   where trimmed.StartsWith(starting)
                   where trimmed.EndsWith(ending)
                   select trimmed;
        }

        private string GenereateVsTestsArguments()
        {
            string arguments = $"\"{_testProjectFolders.CompiledAssemblyPath}\" /logger:trx";

            if (_testRunConfiguration.ProjectFormat == ProjectFormat.Old)
            {
                arguments += $" /TestAdapterPath:\"{_testProjectFolders.PathToNuGetPackages}\"";
            }

            if (Filter.IsNotNullOrEmpty())
            {
                arguments += $" /TestCaseFilter:{Filter}";
            }

            if (RunSettingsFile.IsNotNullOrWhiteSpace())
            {
                arguments += $" /Settings:{RunSettingsFile}";
            }

            return arguments;
        }
    }
}
