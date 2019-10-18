using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;
using Io.Cucumber.Messages;

namespace TechTalk.SpecFlow.TestProjectGenerator.CucumberMessages
{
    public class CucumberMessagesDriver
    {
        private readonly TestProjectFolders _testProjectFolders;

        public CucumberMessagesDriver(TestProjectFolders testProjectFolders)
        {
            _testProjectFolders = testProjectFolders;
        }

        public IMessage UnpackEnvelope(Envelope envelope)
        {
            switch (envelope.MessageCase)
            {
                case Envelope.MessageOneofCase.TestRunStarted: return envelope.TestRunStarted;
                case Envelope.MessageOneofCase.TestCaseStarted: return envelope.TestCaseStarted;
                case Envelope.MessageOneofCase.TestCaseFinished: return envelope.TestCaseFinished;
                case Envelope.MessageOneofCase.TestRunFinished: return envelope.TestRunFinished;
                default: throw new InvalidOperationException($"(Currently) unsupported message type: {envelope.MessageCase}");
            }
        }

        public IEnumerable<IMessage> LoadMessageQueue()
        {
            string pathInBinFolder = Path.Combine(_testProjectFolders.ProjectBinOutputPath, "CucumberMessageQueue", "messages");
            string pathInTestResultsFolder = Path.Combine(_testProjectFolders.ProjectFolder, "TestResults", "CucumberMessageQueue", "messages");
            if (!TryGetPathCucumberMessagesFile(new[] {pathInBinFolder, pathInTestResultsFolder}, out string pathToCucumberMessagesFile))
            {
                yield break;
            }

            using (var fileStream = File.Open(pathToCucumberMessagesFile, FileMode.Open, System.IO.FileAccess.Read))
            {
                while (fileStream.CanSeek && fileStream.Position < fileStream.Length)
                {
                    var messageParser = Envelope.Parser;
                    var message = messageParser.ParseDelimitedFrom(fileStream);
                    yield return UnpackEnvelope(message);
                }
            }
        }

        public bool TryGetPathCucumberMessagesFile(IEnumerable<string> pathsToTry, out string pathToCucumberMessagesQueue)
        {
            string firstPathToBeFoundOrNone = pathsToTry.FirstOrDefault(File.Exists);
            pathToCucumberMessagesQueue = firstPathToBeFoundOrNone;
            return firstPathToBeFoundOrNone is string _;
        }
    }
}
