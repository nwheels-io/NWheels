using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NWheels.Cli
{
    public abstract class CommandBase : ICommand
    {
        protected CommandBase(string name, string helpText)
        {
            this.Name = name;
            this.HelpText = helpText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void DefineArguments(ArgumentSyntax syntax);
        public abstract void ValidateArguments(ArgumentSyntax arguments);
        public abstract void Execute();

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

        protected void Log(string message)
        {
            Console.WriteLine(message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void LogDebug(string message)
        {
            Program.LogMessageWithColor(ConsoleColor.DarkGray, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void LogImportant(string message)
        {
            Program.LogMessageWithColor(ConsoleColor.Cyan, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void LogSuccess(string message)
        {
            Program.LogMessageWithColor(ConsoleColor.Green, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void LogWarning(string message)
        {
            Program.LogMessageWithColor(ConsoleColor.Yellow, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void LogError(string message)
        {
            Program.LogMessageWithColor(ConsoleColor.Red, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ReportFatalError(Exception error)
        {
            Console.Error.WriteLine(error.ToString());
            ReportFatalError(error.Message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ReportFatalError(string message)
        {
            LogError("FATAL ERROR: " + message);
            Environment.Exit(2);
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
