using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NWheels.Kernel.Api.Logging;

namespace NWheels.Microservices.Runtime.Cli
{
    // Built-in:
    //   myservice.exe [--help]
    //   myservice.exe inspect [--debug|--verbose] [--all] [--boot-config] [--config-sections] [--lifecycle-components] [--component-container]
    //   myservice.exe compile
    //   myservice.exe run [--debug|--verbose] [--precompiled] [--cluster <cluster_name> [--partition <partition_index>]] -- [<service_specific_args>...]
    //   myservice.exe job <job_command> [<job_command_specific_args>...] [--debug|--verbose] -- [<job_specific_args>...]
    // Plugged in:
    //   myservice.exe deploy --env <env_name> --env-type <env_type> --to <cloud_provider_name> --credentials <cloud_provider_credentials_kvps>
    //   ...and more...

    public class CliProgram : IDisposable
    {
        private const string BootLogOption = "boot-log";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly CancellationTokenSource _cancellation;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CliProgram()
        {
            _cancellation = new CancellationTokenSource();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Console.CancelKeyPress += OnControlBreakOrSigInt;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _cancellation.Dispose();

            Console.CancelKeyPress -= OnControlBreakOrSigInt;
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Runs command-line interface for a microservice
        /// </summary>
        /// <param name="host">
        /// An instance of MicroserviceHost to run
        /// </param>
        /// <param name="args">
        /// Command line arguments
        /// </param>
        /// <returns>
        /// Exit code to return to the OS: 0 = success, -1 = failed to initialize, -2 = failure during execution
        /// </returns>
        /// <remarks>
        /// If command line arguments are invalid, or help is requested with -h, -?, or --help option, 
        /// this method will print appropriate output, then terminate the process with exit code 1.
        /// </remarks>
        public int Run(MicroserviceHost host, string[] args)
        {
            var commands = host.GetContainer().ResolveAll<ICliCommand>().ToArray();
            var parsedArgs = ParseCommandLine(commands, args);             // will Environment.Exit(1) if not parsed
            var activeCommand = FindActiveCommand(commands, parsedArgs);   // will Environment.Exit(1) if not found
            activeCommand.ValidateArguments(parsedArgs);                   // will Environment.Exit(1) if not valid

            try
            {
                return activeCommand.Execute(_cancellation.Token);
            }
            catch (Exception e)
            {
                ColorConsole.Log(LogLevel.Critical, $"Microservice faulted!{Environment.NewLine}{e}");
                var exitCode = (e is CliCommandFailedException commandFailure ? commandFailure.ExitCode : -2);
                return exitCode;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LogCrash(object fatalError, bool isTerminating)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var terminatingText = (isTerminating ? "TERMINATING" : "non-terminating");
            var errorText = (fatalError?.ToString() ?? "Error is not available");
            var newLine = Environment.NewLine;

            var crashLogMessage = $"{newLine}{timestamp} -- {terminatingText} UNHANDLED EXCEPTION -- {errorText}{newLine}{newLine}";
            var crashLogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");

            File.AppendAllText(crashLogFilePath, crashLogMessage);
            ColorConsole.Log(LogLevel.Critical, crashLogMessage);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnControlBreakOrSigInt(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;

            try
            {
                _cancellation.Cancel(throwOnFirstException: false);
            }
            catch
            {
                // swallow exceptions to avoid crash
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogCrash(e.ExceptionObject, e.IsTerminating);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ArgumentSyntax ParseCommandLine(ICliCommand[] commands, string[] args)
        {
            var safeArgs = args.DefaultIfEmpty("--help");

            return ArgumentSyntax.Parse(safeArgs, syntax => {
                syntax.DefineOption(BootLogOption, defaultValue: false);

                foreach (var command in commands)
                {
                    command.BindToCommandLine(syntax);
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ICliCommand FindActiveCommand(ICliCommand[] commands, ArgumentSyntax parsedArgs)
        {
            ICliCommand activeCommand = null;

            if (parsedArgs.ActiveCommand != null)
            {
                activeCommand = commands.FirstOrDefault(
                    c => string.Equals(c.Name, parsedArgs.ActiveCommand.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (activeCommand == null)
            {
                parsedArgs.ReportError("Command not understood.");
            }

            return activeCommand;
        }
    }
}
