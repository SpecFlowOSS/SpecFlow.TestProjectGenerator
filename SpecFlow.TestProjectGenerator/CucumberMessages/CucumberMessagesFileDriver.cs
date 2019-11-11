using System.IO;
using FluentAssertions;

namespace TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages
{
    public class CucumberMessagesFileDriver
    {
        private readonly CucumberMessagesDriver _cucumberMessagesDriver;
        private readonly TestProjectFolders _testProjectFolders;

        public CucumberMessagesFileDriver(CucumberMessagesDriver cucumberMessagesDriver, TestProjectFolders testProjectFolders)
        {
            _cucumberMessagesDriver = cucumberMessagesDriver;
            _testProjectFolders = testProjectFolders;
        }

        public void CucumberMessagesFileShouldBe(string expectedFileName)
        {
            var pathsToTest =
                Path.IsPathRooted(expectedFileName)
                    ? new[] { expectedFileName }
                    : new [] { Path.Combine(_testProjectFolders.ProjectBinOutputPath, expectedFileName), Path.Combine(_testProjectFolders.ProjectFolder, expectedFileName)};

            bool couldFindFile = _cucumberMessagesDriver.TryGetPathCucumberMessagesFile(pathsToTest, out string _);
            couldFindFile.Should().BeTrue();
        }

        public void CucumberMessagesFileShouldNotExist()
        {
            string pathInBinFolder = Path.Combine(_testProjectFolders.ProjectBinOutputPath, "cucumbermessages", "messages");
            string pathInTestResultsFolder = Path.Combine(_testProjectFolders.ProjectFolder, "TestResults", "cucumbermessages", "messages");
            bool couldFindFile = _cucumberMessagesDriver.TryGetPathCucumberMessagesFile(new[] { pathInBinFolder, pathInTestResultsFolder }, out _);
            couldFindFile.Should().BeFalse();
        }
    }
}
