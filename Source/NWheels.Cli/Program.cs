using System;
using System.CommandLine;
using System.Linq;

namespace NWheels.Cli
{
    class Program
    {
        private static readonly ICommand[] _s_commands = new[] {
            new Service.ServiceCommand()
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static int Main(string[] args)
        {
            ArgumentSyntax arguments = ParseCommandLine(args);      // will exit with code 1 if not parsed
            ICommand activeCommand = FindActiveCommand(arguments);  // will exit with code 1 if not found
            activeCommand.ValidateArguments(arguments);             // will exit with code 1 if not valid

            int exitCode = 0;

            try
            {
                activeCommand.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                exitCode = 2;
            }

            return exitCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ArgumentSyntax ParseCommandLine(string[] args)
        {
            return ArgumentSyntax.Parse(args, syntax => {
                foreach (var command in _s_commands)
                {
                    var commandSyntax = syntax.DefineCommand(command.Name);
                    commandSyntax.Help = command.HelpText;
                    command.DefineArguments(syntax);
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