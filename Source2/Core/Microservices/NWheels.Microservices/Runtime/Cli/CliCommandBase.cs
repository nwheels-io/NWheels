using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace NWheels.Microservices.Runtime.Cli
{
    public abstract class CliCommandBase : ICliCommand
    {
        protected CliCommandBase(string name, string helpText)
        {
            this.Name = name;
            this.HelpText = helpText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void DefineArguments(ArgumentSyntax syntax);
        public abstract void ValidateArguments(ArgumentSyntax arguments);
        public abstract int Execute(CancellationToken cancellation);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; }
        public string HelpText { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected int ExecuteProgram(
            string nameOrFilePath,
            string[] args = null,
            string workingDirectory = null,
            bool validateExitCode = true)
        {
            return ExecuteProgram(
                out IEnumerable<string> output,
                nameOrFilePath,
                args,
                workingDirectory,
                validateExitCode,
                shouldInterceptOutput: false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected int ExecuteProgram(
            out IEnumerable<string> output,
            string nameOrFilePath,
            string[] args = null,
            string workingDirectory = null,
            bool validateExitCode = true)
        {
            return ExecuteProgram(
                out output,
                nameOrFilePath,
                args,
                workingDirectory,
                validateExitCode,
                shouldInterceptOutput: true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private int ExecuteProgram(
            out IEnumerable<string> output,
            string nameOrFilePath,
            string[] args,
            string workingDirectory,
            bool validateExitCode,
            bool shouldInterceptOutput)
        {
            var info = new ProcessStartInfo() {
                FileName = nameOrFilePath,
                Arguments = (args != null ? string.Join(" ", args) : string.Empty),
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = shouldInterceptOutput
            };

            var process = Process.Start(info);
            List<string> outputLines = null;

            if (shouldInterceptOutput)
            {
                outputLines = new List<string>(capacity: 100);
                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    outputLines.Add(line);
                }
            }

            process.WaitForExit();
            output = outputLines;

            if (validateExitCode && process.ExitCode != 0)
            {
                throw new Exception(
                    $"Program '{nameOrFilePath}' failed with code {process.ExitCode}." +
                    (outputLines != null ? Environment.NewLine + string.Join(Environment.NewLine, outputLines) : string.Empty));
            }

            return process.ExitCode;
        }
    }
}
