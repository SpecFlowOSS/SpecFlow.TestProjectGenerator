using System.Linq;
using FluentAssertions;
using Io.Cucumber.Messages;

namespace TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages
{
    public class TestRunFinishedDriver
    {
        private readonly CucumberMessagesDriver _cucumberMessagesDriver;

        public TestRunFinishedDriver(CucumberMessagesDriver cucumberMessagesDriver)
        {
            _cucumberMessagesDriver = cucumberMessagesDriver;
        }

        public void TestRunFinishedMessageShouldHaveBeenSent(int numberOfMessages)
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();
            messageQueue.ToArray().OfType<TestRunStarted>().Should().HaveCount(numberOfMessages);
        }
    }
}