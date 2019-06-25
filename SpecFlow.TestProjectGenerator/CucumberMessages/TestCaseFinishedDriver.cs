using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Io.Cucumber.Messages;
using TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages.RowObjects;

namespace TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages
{
    public class TestCaseFinishedDriver
    {
        private readonly CucumberMessagesDriver _cucumberMessagesDriver;

        public TestCaseFinishedDriver(CucumberMessagesDriver cucumberMessagesDriver)
        {
            _cucumberMessagesDriver = cucumberMessagesDriver;
        }

        public void TestCaseFinishedMessagesShouldHaveBeenSent(int amount)
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();
            messageQueue.ToArray().OfType<TestCaseFinished>().Should().HaveCount(amount);
        }

        public void TestCaseFinishedMessageShouldHaveBeenSent(TestCaseFinishedRow testCaseFinishedRow)
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();

            var testCaseFinishedMessages = messageQueue.OfType<TestCaseFinished>().ToArray();
            testCaseFinishedMessages.Should().HaveCountGreaterOrEqualTo(1);

            var testCaseFinished = testCaseFinishedMessages.First();

            if (testCaseFinishedRow.Timestamp is string expectedTimeStampString
                && DateTime.TryParse(expectedTimeStampString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var expectedTimeStamp))
            {
                testCaseFinished.Timestamp.ToDateTime().Should().Be(expectedTimeStamp);
            }

            if (testCaseFinishedRow.PickleId is string expectedPickleId)
            {
                testCaseFinished.PickleId.Should().Be(expectedPickleId);
            }
        }

        public void TestCaseFinishedMessageShouldHaveBeenSentWithTestResult(TestResultRow testResultRow)
        {
            var messageQueue = _cucumberMessagesDriver.LoadMessageQueue();

            var testCaseFinishedMessages = messageQueue.OfType<TestCaseFinished>().ToArray();
            testCaseFinishedMessages.Should().HaveCountGreaterOrEqualTo(1);

            var testCaseFinished = testCaseFinishedMessages.First();
            var testResult = testCaseFinished.TestResult;

            if (testResultRow.Status is Status expectedStatus)
            {
                testResult.Status.Should().Be(expectedStatus);
            }
        }
    }
}
