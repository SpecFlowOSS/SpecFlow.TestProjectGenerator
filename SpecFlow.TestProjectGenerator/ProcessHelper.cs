using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class ProcessHelper
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(15);
        private static readonly int _timeOutInMilliseconds = Convert.ToInt32(_timeout.TotalMilliseconds);

        public ProcessResult RunProcess(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, IReadOnlyDictionary<string, string> environmentVariables, params object[] arguments)
        {
            string parameters = string.Format(argumentsFormat, arguments);

            outputWriter.WriteLine("Starting external program: \"{0}\" {1} in {2}", executablePath, parameters, workingDirectory);
            var psi = CreateProcessStartInfo(workingDirectory, executablePath, parameters, environmentVariables);

            using (var process = new Process { StartInfo = psi, EnableRaisingEvents = true })
            {
                var stdError = new StringBuilder();

                process.ErrorDataReceived += (sender, e) => { stdError.Append(e.Data); };

                var before = DateTime.Now;
                process.Start();

                process.BeginErrorReadLine();

                string stdOutput = process.StandardOutput.ReadToEnd();
                bool processResult = process.WaitForExit(_timeOutInMilliseconds);

                if (!processResult)
                {
                    throw new TimeoutException(
                        $"Process {psi.FileName} {psi.Arguments} took longer than {_timeout.TotalMinutes} min to complete." + Environment.NewLine + "Std Output:" + Environment.NewLine + stdOutput);
                }

                var after = DateTime.Now;
                var diff = after - before;
                outputWriter.WriteLine($"'{executablePath} {parameters}' took {diff.TotalMilliseconds}ms");

                outputWriter.WriteLine($"StdOutput: {stdOutput}");
                return new ProcessResult(process.ExitCode, stdOutput, stdError.ToString(), $"{stdOutput}{stdError}");
            }
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
