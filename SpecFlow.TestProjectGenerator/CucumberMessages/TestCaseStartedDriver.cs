using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Io.Cucumber.Messages;
using TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages.RowObjects;

namespace TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages
{
    public class TestCaseStartedDriver
    {
        private readonly CucumberMessagesDriver _cucumberMessagesDriver;

        public TestCaseStartedDriver(CucumberMessagesDriver cucumberMessagesDriver)
        {
            _cucumberMessagesDriver = cucumberMessagesDriver;
        }

        public void TestCaseStartedMessagesShouldHaveBeenSent(int amount)
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();
            messageQueue.ToArray().OfType<TestCaseStarted>().Should().HaveCount(amount);
        }

        public void TestCaseStartedMessageShouldHaveBeenSent(TestCaseStartedRow testCaseStartedRow)
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();
            var testCaseStarted = messageQueue.ToArray().OfType<TestCaseStarted>().First();
            if (testCaseStartedRow.Timestamp is string expectedTimeStampString
                && DateTime.TryParse(expectedTimeStampString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var expectedTimeStamp))
            {
                testCaseStarted.Timestamp.ToDateTime().Should().Be(expectedTimeStamp);
            }

            if (testCaseStartedRow.PickleId is string expectedPickleId)
            {
                testCaseStarted.PickleId.Should().Be(expectedPickleId);
            }
        }

        public void TestCaseStartedMessageShouldHaveBeenSentWithPlatformInformation(PlatformRow platformRow)
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();
            var testCaseStarted = messageQueue.ToArray().OfType<TestCaseStarted>().First();

            if (platformRow.Cpu is string cpu)
            {
                testCaseStarted.Platform.Cpu.Should().Be(cpu);
            }

            if (platformRow.Os is string os)
            {
                testCaseStarted.Platform.Os.Should().Be(os);
            }

            if (platformRow.Implementation is string implementation)
            {
                testCaseStarted.Platform.Implementation.Should().Be(implementation);
            }

            if (platformRow.Version is string version)
            {
                testCaseStarted.Platform.Version.Should().Be(version);
            }
        }
    }
}
