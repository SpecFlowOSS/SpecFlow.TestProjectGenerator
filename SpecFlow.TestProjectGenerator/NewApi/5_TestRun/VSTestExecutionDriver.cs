using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentAssertions;
using SpecFlow.TestProjectGenerator.Helpers;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi._5_TestRun
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

        public void CheckIsBindingMethodExecuted(string methodName, int timesExecuted)
        {
            LastTestExecutionResult.Should().NotBeNull();
            var regex = new Regex($@"-> done: \S+\.{methodName}");

            regex.Match(LastTestExecutionResult.Output).Success.Should().BeTrue($"method {methodName} was not executed.");
            regex.Matches(LastTestExecutionResult.Output).Count.Should().Be(timesExecuted);
        }

        public void CheckOutputContainsText(string text)
        {
            LastTestExecutionResult.Output.Should().NotBeNull()
                                   .And.Subject.Should().Contain(text);
        }

        public void ExecuteTests(string tag = null)
        {
            string vsFolder = _visualStudioFinder.Find();
            vsFolder = Path.Combine(vsFolder, _appConfigDriver.VSTestPath);

            var vsTestConsoleExePath = Path.Combine(AssemblyFolderHelper.GetTestAssemblyFolder(), Environment.ExpandEnvironmentVariables(vsFolder + @"\vstest.console.exe"));

            var processHelper = new ProcessHelper();
            string arguments = GenereateVsTestsArguments(tag != null ? $"Category={tag}" : null);
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

            if (trxFiles.Count() != 1)
                throw new Exception("No or to many trx files in output found!" + Environment.NewLine + string.Join(Environment.NewLine, trxFiles));


            var trxFile = trxFiles.Single().Substring(BeginnOfTrxFileLine.Length);
            var testResult = XDocument.Load(trxFile);

            TestExecutionResult executionResult = new TestExecutionResult();

            XmlNameTable nameTable = new NameTable();
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(nameTable);
            namespaceManager.AddNamespace("mstest", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

            bool isXUnit = output.Contains("xUnit.net");

            var summaryElement = testResult.XPathSelectElement("//mstest:ResultSummary/mstest:Counters", namespaceManager);
            if (summaryElement != null)
            {
                executionResult.Total = int.Parse(summaryElement.Attribute("total").Value);
                executionResult.Executed = int.Parse(summaryElement.Attribute("executed").Value);
                executionResult.Succeeded = int.Parse(summaryElement.Attribute("passed").Value);
                executionResult.Pending = isXUnit ? GetXUnitPendingCount(output) : int.Parse(summaryElement.Attribute("inconclusive").Value);
                executionResult.Failed = int.Parse(summaryElement.Attribute("failed").Value) - executionResult.Pending;
                executionResult.Ignored = executionResult.Total - executionResult.Executed;
                executionResult.Output = output;
            }

            LastTestExecutionResult = executionResult;
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

        private string GenereateVsTestsArguments(string filter)
        {
            string arguments = $"\"{_testProjectFolders.CompiledAssemblyPath}\" /logger:trx";

            if (_testRunConfiguration.ProjectFormat == ProjectFormat.Old)
            {
                arguments += $" /TestAdapterPath:\"{_testProjectFolders.PathToNuGetPackages}\"";
            }

            if (filter.IsNotNullOrEmpty())
            {
                arguments += $" /TestCaseFilter:{filter}";
            }

            return arguments;
        }
    }
}
