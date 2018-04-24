using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using SpecFlow.TestProjectGenerator.Helpers;

namespace SpecFlow.TestProjectGenerator.NewApi._5_TestRun
{
    public class VSTestExecution
    {
        private readonly VisualStudioFinder _visualStudioFinder;
        private readonly AppConfigDriver _appConfigDriver;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly Folders _folders;

        private const string BeginnOfTrxFileLine = "Results File: ";
        private const string BeginnOfLogFileLine = "Log file: ";

        public VSTestExecution(VisualStudioFinder visualStudioFinder, AppConfigDriver appConfigDriver, TestProjectFolders testProjectFolders, Folders folders)
        {
            _visualStudioFinder = visualStudioFinder;
            _appConfigDriver = appConfigDriver;
            _testProjectFolders = testProjectFolders;
            _folders = folders;
        }

        public TestExecutionResult ExecuteTests()
        {
            string vsFolder = _visualStudioFinder.Find();

            vsFolder = Path.Combine(vsFolder, _appConfigDriver.VSTestPath);

            var vsTestConsoleExePath = Path.Combine(AssemblyFolderHelper.GetTestAssemblyFolder(), Environment.ExpandEnvironmentVariables(vsFolder + @"\vstest.console.exe"));

            var processHelper = new ProcessHelper();



            string arguments = GenereateVsTestsArguments(null);
            ProcessResult processResult;
            try
            {
                processResult = processHelper.RunProcess(_testProjectFolders.ProjectFolder, vsTestConsoleExePath, arguments);
            }
            catch (Exception)
            {
                Console.WriteLine($"running vstest.console.exe failed - {_testProjectFolders.CompiledAssemblyPath} {vsTestConsoleExePath} {arguments}");
                throw;
            }


            var output = processResult.CombinedOutput;

            var lines = output.SplitByString(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var trxFiles = FindFilePath(lines, ".trx", BeginnOfTrxFileLine);

            if (trxFiles.Count() != 1)
                throw new Exception("No or to many trx files in output found!" + Environment.NewLine + String.Join(Environment.NewLine, trxFiles));


            var trxFile = trxFiles.Single().Substring(BeginnOfTrxFileLine.Length);
            var testResult = XDocument.Load(trxFile);



            TestExecutionResult executionResult = new TestExecutionResult();

            XmlNameTable nameTable = new NameTable();
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(nameTable);
            namespaceManager.AddNamespace("mstest", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

            var summaryElement = testResult.XPathSelectElement("//mstest:ResultSummary/mstest:Counters", namespaceManager);
            if (summaryElement != null)
            {
                executionResult.Total = int.Parse(summaryElement.Attribute("total").Value);
                executionResult.TotalSucceeded = int.Parse(summaryElement.Attribute("passed").Value);
                executionResult.TotalFailure = int.Parse(summaryElement.Attribute("failed").Value);
                executionResult.TotalPending = int.Parse(summaryElement.Attribute("inconclusive").Value);
                executionResult.TotalIgnored = 0; // mstest does not support ignored in the report
                executionResult.Output = output;
            }

            return executionResult;
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
            string arguments = $"\"{_testProjectFolders.CompiledAssemblyPath}\" /logger:trx /TestAdapterPath:\"{_testProjectFolders.PathToNuGetPackages}\"";

            if (filter.IsNotNullOrEmpty())
            {
                arguments += $" /TestCaseFilter:{filter}";
            }

            return arguments;
        }
    }
}
