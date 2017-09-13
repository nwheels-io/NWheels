using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Microservices.Runtime
{
    public static class MicroserviceHostExtensions
    {
        public static int Run(this MicroserviceHost host, string[] commandLineArgs)
        {
            var ctrlBreakPressed = WaitForCtrlBreakAsync();

            try
            {
                host.Configure();
                host.LoadAndActivate();

                PrintMessageToConsole(
                    ConsoleColor.Green, 
                    $"Microservice is up.{Environment.NewLine}Press ENTER or CTRL+C to go down.");

                var enterPressed = Console.In.ReadLineAsync();
                Task.WaitAny(enterPressed, ctrlBreakPressed);

                host.DeactivateAndUnload();

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                PrintMessageToConsole(ConsoleColor.Red, $"FATAL ERROR: {e.Message}");
                return 2;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Task WaitForCtrlBreakAsync()
        {
            var waitingForCtrlBreak = new TaskCompletionSource<bool>();

            Console.CancelKeyPress += (sender, cancelArgs) => {
                cancelArgs.Cancel = true;
                waitingForCtrlBreak.SetResult(true);
            };

            return waitingForCtrlBreak.Task;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void PrintMessageToConsole(ConsoleColor color, string message)
        {
            var saveColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = saveColor;
        }
    }
}
