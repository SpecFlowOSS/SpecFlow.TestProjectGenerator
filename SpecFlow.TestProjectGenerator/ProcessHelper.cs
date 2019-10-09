using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi;

namespace TechTalk.SpecFlow.TestProjectGenerator
{

    public class ProcessResult
    {
        public ProcessResult(int exitCode, string stdOutput, string stdError, string combinedOutput)
        {
            ExitCode = exitCode;
            StdOutput = stdOutput;
            StdError = stdError;
            CombinedOutput = combinedOutput;
        }

        public string StdOutput { get; }
        public string StdError { get; }
        public string CombinedOutput { get; }
        public int ExitCode { get; }
    }

    public class ProcessHelper
    {
        private static TimeSpan _timeout = TimeSpan.FromMinutes(15);
        private static readonly int _timeOutInMilliseconds = Convert.ToInt32(_timeout.TotalMilliseconds);

        public ProcessResult RunProcess(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, IReadOnlyDictionary<string, string> environmentVariables, params object[] arguments)
        {
            string parameters = string.Format(argumentsFormat, arguments);

            outputWriter.WriteLine("Starting external program: \"{0}\" {1} in {2}", executablePath, parameters, workingDirectory);
            var psi = CreateProcessStartInfo(workingDirectory, executablePath, parameters, environmentVariables);


            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true,

            };

            StringBuilder combinedOutput = new StringBuilder();
            StringBuilder stdOutput = new StringBuilder();
            StringBuilder stdError = new StringBuilder();


            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            {
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            combinedOutput.AppendLine(e.Data);
                            stdOutput.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            combinedOutput.AppendLine(e.Data);
                            stdError.AppendLine(e.Data);
                        }
                    };

                    var before = DateTime.Now;
                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (!process.WaitForExit(_timeOutInMilliseconds))
                    {
                        throw new TimeoutException($"Process {psi.FileName} {psi.Arguments} took longer than {_timeout.TotalMinutes} min to complete." + Environment.NewLine + "Combined Output:" + Environment.NewLine + combinedOutput);
                    }

                    var after = DateTime.Now;
                    var diff = after - before;
                    outputWriter.WriteLine($"'{executablePath} {parameters}' took {diff.TotalMilliseconds}ms");
                }
            }

            outputWriter.WriteLine(combinedOutput.ToString());

            return new ProcessResult(process.ExitCode, stdOutput.ToString(), stdError.ToString(), combinedOutput.ToString());
        }

        public ProcessResult RunProcess(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, params object[] arguments)
        {
            return RunProcess(outputWriter, workingDirectory, executablePath, argumentsFormat, new Dictionary<string, string>(), arguments);
        }

        private ProcessStartInfo CreateProcessStartInfo(string workingDirectory, string executablePath, string parameters, IReadOnlyDictionary<string, string> environmentVariables)
        {
            var processStartInfo = new ProcessStartInfo(executablePath, parameters)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory
            };

            foreach (var env in environmentVariables)
            {
                processStartInfo.Environment.Add(env.Key, env.Value);
            }

            return processStartInfo;
        }
    }
}