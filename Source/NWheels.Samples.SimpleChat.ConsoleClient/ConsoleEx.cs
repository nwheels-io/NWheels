using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Samples.SimpleChat.ConsoleClient
{
    public static class ConsoleEx
    {
        public static void WriteLine(ConsoleColor color, string message, params object[] args)
        {
            var saveColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message.FormatIf(args));
            Console.ForegroundColor = saveColor;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static async Task<string> ReadLineAsync(ConsoleColor color, string prompt, params object[] args)
        {
            var saveColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(prompt.FormatIf(args));

            var input = await Task.Run(() => Console.ReadLine());
            
            Console.ForegroundColor = saveColor;
            
            return input;
        }
    }
}
