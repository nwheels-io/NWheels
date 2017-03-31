using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
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

        protected int ExecuteProgram(string nameOrFilePath, string[] args, string workingDirectory = null, bool validateExitCode = true)
        {
            var info = new ProcessStartInfo() {
                FileName = nameOrFilePath,
                Arguments = string.Join(" ", args),
                WorkingDirectory = workingDirectory
            };

            var process = Process.Start(info);
            process.WaitForExit();

            if (validateExitCode && process.ExitCode != 0)
            {
                throw new Exception($"Program '{nameOrFilePath}' failed with code {process.ExitCode}.");
            }

            return process.ExitCode;
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

        protected void LogFatal(string message)
        {
            LogError("FATAL ERROR: " + message);
            Environment.Exit(2);
        }
    }
}
