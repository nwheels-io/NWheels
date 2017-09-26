using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Logging;

namespace NWheels.Microservices.Runtime.Cli
{
    public static class ColorConsole
    {
        public static void Log(LogLevel level, string message)
        {
            ConsoleColor color = GetLogLevelColor(level);
            WriteLine(color, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ConsoleColor GetLogLevelColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return ConsoleColor.DarkGray;
                case LogLevel.Info:
                    return ConsoleColor.Cyan;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.DarkRed;
                case LogLevel.Critical:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.Gray; // LogLevel.Verbose
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void WriteLine(ConsoleColor color, string message)
        {
            var saveColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = saveColor;
        }
    }
}
