using System;
using System.CommandLine;
using System.Linq;

namespace NWheels.Cli
{
    class Program
    {
        private static ICommand[] _s_commands;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static int Main(string[] args)
        {
            var context = new RealCommandContext();

            _s_commands = new ICommand[] {
                new Publish.PublishCommand(context),
                new Run.RunCommand(context)
            };

            ArgumentSyntax arguments = ParseCommandLine(args);      // will exit with code 1 if not parsed
            ICommand activeCommand = FindActiveCommand(arguments);  // will exit with code 1 if not found

            int exitCode = 0;

            try
            {
                activeCommand.ValidateArguments();  // will throw BadArgumentsException if invalid
                activeCommand.Execute();
            }
            catch (BadArgumentsException e)
            {
                Console.WriteLine(e.Message);
                exitCode = 1;
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