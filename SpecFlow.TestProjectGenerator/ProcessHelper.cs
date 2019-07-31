using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class ProcessHelper
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(15);
        private static readonly int _timeOutInMilliseconds = Convert.ToInt32(_timeout.TotalMilliseconds);

        public delegate Task<TResult> ProcessResultSelector<TResult>(IOutputWriter outputWriter, string executablePath, Process process, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle,
            ProcessStartInfo psi, StringBuilder combinedOutput, DateTime before, string parameters, StringBuilder stdOutput, StringBuilder stdError);

        public ProcessResult RunProcess(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, IReadOnlyDictionary<string, string> environmentVariables,
            params object[] arguments)
        {
            var task = RunProcessInternal(outputWriter, workingDirectory, executablePath, argumentsFormat, environmentVariables,
                async (writer, s, process, @event, handle, psi, output, time, parameters, builder, error) =>
                    WaitForProcessResult(writer, s, process, @event, handle, psi, output, time, parameters, builder, error), arguments);

            return task.Result;
        }

        public async Task<ProcessResult> RunProcessAsync(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, IReadOnlyDictionary<string, string> environmentVariables,
            params object[] arguments)
        {
            return await RunProcessInternal(outputWriter, workingDirectory, executablePath, argumentsFormat, environmentVariables, WaitForProcessResultAsync, arguments);
        }

        private async Task<TResult> RunProcessInternal<TResult>(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, IReadOnlyDictionary<string, string> environmentVariables,
            ProcessResultSelector<TResult> processResultSelector, params object[] arguments)
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

                    return await processResultSelector(outputWriter, executablePath, process, outputWaitHandle, errorWaitHandle, psi, combinedOutput, before, parameters, stdOutput, stdError);
                }
            }
        }

        private static ProcessResult WaitForProcessResult(IOutputWriter outputWriter, string executablePath, Process process, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle,
            ProcessStartInfo psi, StringBuilder combinedOutput, DateTime before, string parameters, StringBuilder stdOutput, StringBuilder stdError)
        {
            if (!process.WaitForExit(_timeOutInMilliseconds) || !outputWaitHandle.WaitOne(_timeOutInMilliseconds) ||
                !errorWaitHandle.WaitOne(_timeOutInMilliseconds))
            {
                throw new TimeoutException($"Process {psi.FileName} {psi.Arguments} took longer than {_timeout.TotalMinutes} min to complete." + Environment.NewLine + "Combined Output:" +
                                           Environment.NewLine + combinedOutput);
            }

            var after = DateTime.Now;
            var diff = after - before;
            outputWriter.WriteLine($"'{executablePath} {parameters}' took {diff.TotalMilliseconds}ms");

            outputWriter.WriteLine(combinedOutput.ToString());

            return new ProcessResult(process.ExitCode, stdOutput.ToString(), stdError.ToString(), combinedOutput.ToString());
        }

        private static async Task<ProcessResult> WaitForProcessResultAsync(IOutputWriter outputWriter, string executablePath, Process process, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle,
            ProcessStartInfo psi, StringBuilder combinedOutput, DateTime before, string parameters, StringBuilder stdOutput, StringBuilder stdError)
        {
            var taskCompletionSource = new TaskCompletionSource<ProcessResult>();
            var cancellationTokenSource = new CancellationTokenSource(_timeOutInMilliseconds);
            
            cancellationTokenSource.Token.Register(() =>
            {
                taskCompletionSource.TrySetException(new TimeoutException($"Process {psi.FileName} {psi.Arguments} took longer than {_timeout.TotalMinutes} min to complete." + Environment.NewLine +
                                                                       "Combined Output:" +
                                                                       Environment.NewLine + combinedOutput));
            }, useSynchronizationContext: false);

            process.Exited += (sender, args) =>
            {
                var after = DateTime.Now;
                var diff = after - before;
                outputWriter.WriteLine($"'{executablePath} {parameters}' took {diff.TotalMilliseconds}ms");

                outputWriter.WriteLine(combinedOutput.ToString());

                taskCompletionSource.TrySetResult(new ProcessResult(process.ExitCode, stdOutput.ToString(), stdError.ToString(), combinedOutput.ToString()));
            };

            return await taskCompletionSource.Task;
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