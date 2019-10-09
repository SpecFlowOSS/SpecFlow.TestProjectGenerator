using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;
using TechTalk.SpecFlow.TestProjectGenerator.Helpers;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class VSTestExecutionDriver
    {
        private readonly VisualStudioFinder _visualStudioFinder;
        private readonly AppConfigDriver _appConfigDriver;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly IOutputWriter _outputWriter;
        private readonly TestRunConfiguration _testRunConfiguration;
        private readonly TestSuiteInitializationDriver _testSuiteInitializationDriver;
        private UriCleaner _uriCleaner;
        private readonly TRXParser _trxParser;

        private const string BeginnOfTrxFileLine = "Results File: ";
        private const string BeginnOfLogFileLine = "Log file: ";

        public VSTestExecutionDriver(VisualStudioFinder visualStudioFinder, AppConfigDriver appConfigDriver, TestProjectFolders testProjectFolders, IOutputWriter outputWriter, TestRunConfiguration testRunConfiguration, TestSuiteInitializationDriver testSuiteInitializationDriver, TRXParser trxParser)
        {
            _visualStudioFinder = visualStudioFinder;
            _appConfigDriver = appConfigDriver;
            _testProjectFolders = testProjectFolders;
            _outputWriter = outputWriter;
            _testRunConfiguration = testRunConfiguration;
            _testSuiteInitializationDriver = testSuiteInitializationDriver;
            _uriCleaner = new UriCleaner();
            _trxParser = trxParser;
        }

        public TestExecutionResult LastTestExecutionResult { get; private set; }
        public string RunSettingsFile { get; set; }
        public string Filter { get; set; }

        public void CheckIsBindingMethodExecuted(string methodName, int timesExecuted)
        {
            string pathToLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");
            string logFileContent = File.ReadAllText(pathToLogFile, Encoding.UTF8);

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

            string vsTestConsoleExePath = Path.Combine(AssemblyFolderHelper.GetAssemblyFolder(), Environment.ExpandEnvironmentVariables(vsFolder + @"\vstest.console.exe"));

            var envVariables = new Dictionary<string, string>();

            if (_testSuiteInitializationDriver.OverrideTestSuiteStartupTime is DateTime testRunStartupTime)
            {
                envVariables.Add("SpecFlow_Messages_TestRunStartedTimeOverride", $"{testRunStartupTime:O}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseStartedPickleId is Guid startedPickleId)
            {
                envVariables.Add("SpecFlow_Messages_TestCaseStartedPickleIdOverride", $"{startedPickleId:D}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseStartedTime is DateTime testCaseStartupTime)
            {
                envVariables.Add("SpecFlow_Messages_TestCaseStartedTimeOverride", $"{testCaseStartupTime:O}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseFinishedPickleId is Guid finishedPickleId)
            {
                envVariables.Add("SpecFlow_Messages_TestCaseFinishedPickleIdOverride", $"{finishedPickleId:D}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseFinishedTime is DateTime testCaseFinishedTime)
            {
                envVariables.Add("SpecFlow_Messages_TestCaseFinishedTimeOverride", $"{testCaseFinishedTime:O}");
            }

            var processHelper = new ProcessHelper();
            string arguments = GenereateVsTestsArguments();
            ProcessResult processResult;
            try
            {
                processResult = processHelper.RunProcess(_outputWriter, _testProjectFolders.ProjectFolder, vsTestConsoleExePath, arguments, envVariables);
            }
            catch (Exception)
            {
                Console.WriteLine($"running vstest.console.exe failed - {_testProjectFolders.CompiledAssemblyPath} {vsTestConsoleExePath} {arguments}");
                throw;
            }

            string output = processResult.CombinedOutput;

            var lines = output.SplitByString(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var trxFiles = FindFilePath(lines, ".trx", BeginnOfTrxFileLine).ToArray();
            var logFiles = FindFilePath(lines, ".log", BeginnOfLogFileLine).ToArray();



            string logFileContent =
                logFiles.Length == 1
                ? File.ReadAllText(_uriCleaner.ConvertSlashes(_uriCleaner.StripSchema(Uri.UnescapeDataString(logFiles.Single()))))
                : string.Empty;

            var reportFiles = GetReportFiles(output);

            trxFiles.Should().HaveCount(1, $"exactly one TRX file should be generated by VsTest;{Environment.NewLine}{string.Join(Environment.NewLine, trxFiles)}");
            string trxFile = trxFiles.Single();

            LastTestExecutionResult = _trxParser.ParseTRXFile(trxFile, output, reportFiles, logFileContent);
            return LastTestExecutionResult;
        }

        private IEnumerable<string> GetReportFiles(string output)
        {
            const string reportFileString = @"Report file: ";

            return output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                         .Where(i => i.StartsWith(reportFileString))
                         .Select(i => i.Substring(reportFileString.Length))
                         .Select(i => new Uri(i).AbsolutePath);
        }

        private IEnumerable<string> FindFilePath(string[] lines, string ending, string starting)
        {
            return from l in lines
                   let trimmed = l.Trim()
                   let start = trimmed.IndexOf(starting)
                   where trimmed.Contains(starting)
                   where trimmed.EndsWith(ending)
                   select trimmed.Substring(start + starting.Length);
        }

        private string GenereateVsTestsArguments()
        {
            string arguments = $"\"{_testProjectFolders.CompiledAssemblyPath}\" /logger:trx";

            if (_testRunConfiguration.UnitTestProvider != UnitTestProvider.SpecRun)
            {
                if (_testRunConfiguration.ProjectFormat == ProjectFormat.Old)
                {
                    arguments += $" /TestAdapterPath:\"{_testProjectFolders.PathToNuGetPackages}\"";
                }
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
