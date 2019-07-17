using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Io.Cucumber.Messages;
using TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages.RowObjects;

namespace TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages
{
    public class TestRunStartedDriver
    {
        private readonly CucumberMessagesDriver _cucumberMessagesDriver;

        public TestRunStartedDriver(CucumberMessagesDriver cucumberMessagesDriver)
        {
            _cucumberMessagesDriver = cucumberMessagesDriver;
        }

        public void TestRunStartedMessageShouldHaveBeenSent()
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();
            messageQueue.ToArray().Should().Contain(m => m is TestRunStarted);
        }

        public void TestRunStartedMessageShouldHaveBeenSent(int amount)
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();
            messageQueue.ToArray().OfType<TestRunStarted>().Should().HaveCount(amount);
        }

        public void TestRunStartedMessageShouldHaveBeenSent(TestRunStartedRow testRunStartedRow)
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();
            var testRunStarted = messageQueue.ToArray()
                                             .Should()
                                             .Contain(m => m is TestRunStarted)
                                             .Which.Should()
                                             .BeOfType<TestRunStarted>()
                                             .Which;

            if (testRunStartedRow.Timestamp is string expectedTimeStampString
                && DateTime.TryParse(expectedTimeStampString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var expectedTimeStamp))
            {
                testRunStarted.Timestamp.ToDateTime().Should().Be(expectedTimeStamp);
            }
        }
    }
}
