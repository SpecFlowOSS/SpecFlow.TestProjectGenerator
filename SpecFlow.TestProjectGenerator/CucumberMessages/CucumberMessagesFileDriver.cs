using FluentAssertions;

namespace TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages
{
    public class CucumberMessagesFileDriver
    {
        private readonly CucumberMessagesDriver _cucumberMessagesDriver;

        public CucumberMessagesFileDriver(CucumberMessagesDriver cucumberMessagesDriver)
        {
            _cucumberMessagesDriver = cucumberMessagesDriver;
        }

        public void CucumberMessagesFileShouldBe(string expectedFileName)
        {
            bool couldFindFile = _cucumberMessagesDriver.TryGetPathCucumberMessagesFile(out string actualFileName);
            couldFindFile.Should().BeTrue();
            actualFileName.Should().Be(expectedFileName);
        }

        public void CucumberMessagesFileShouldNotExist()
        {
            bool couldFindFile = _cucumberMessagesDriver.TryGetPathCucumberMessagesFile(out _);
            couldFindFile.Should().BeFalse();
        }
    }
}
