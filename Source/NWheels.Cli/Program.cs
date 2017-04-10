using System;
using System.CommandLine;
using System.Linq;

namespace NWheels.Cli
{
    class Program
    {
        private static readonly ICommand[] _s_commands = {
            new Publish.PublishCommand(),
            new Run.RunCommand()
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static int Main(string[] args)
        {
            var arguments = ParseCommandLine(args);             // will Environment.Exit with code 1 if not parsed
            var activeCommand = FindActiveCommand(arguments);   // will Environment.Exit with code 1 if not found
            activeCommand.ValidateArguments(arguments);         // will Environment.Exit with code 1 if not valid

            var exitCode = 0;

            try
            {
                activeCommand.Execute();
            }
            catch (Exception e)
            {
                LogMessageWithColor(ConsoleColor.Red, "FAILED: " + e.Message);
                Console.Error.WriteLine(e.ToString());
                exitCode = 2;
            }

            return exitCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LogMessageWithColor(ConsoleColor color, string message)
        {
            var saveColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = saveColor;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ArgumentSyntax ParseCommandLine(string[] args)
        {
            var safeArgs = args.DefaultIfEmpty("--help");

            return ArgumentSyntax.Parse(safeArgs, syntax => {
                foreach (var command in _s_commands)
                {
                    command.BindToCommandLine(syntax);
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ICommand FindActiveCommand(ArgumentSyntax arguments)
        {
            ICommand activeCommand = null;

            if (arguments.ActiveCommand != null)
            {
                activeCommand = _s_commands
                    .FirstOrDefault(
                        c => string.Equals(c.Name, arguments.ActiveCommand.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (activeCommand == null)
            {
                arguments.ReportError("Command not understood.");
            }

            return activeCommand;
        }
    }
}