using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NWheels.Kernel.Api.Logging;

namespace NWheels.Microservices.Runtime.Cli
{
    public static class ColorConsole
    {
        private static readonly DateTime _timeSeed = DateTime.Now;
        private static readonly Stopwatch _clock = Stopwatch.StartNew();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Log(LogLevel level, string message)
        {
            var color = GetLogLevelColor(level);
            var timestamp = _timeSeed.Add(_clock.Elapsed);

            WriteLine(color, $"{timestamp:dd-MMM HH:mm:ss.fff} {message}");

            if (level >= LogLevel.Warning)
            {
                Debug.WriteLine(message);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LogHeading(string text)
        {
            var timestamp = _timeSeed.Add(_clock.Elapsed);
            WriteLine(ConsoleColor.Magenta, $"{timestamp:dd-MMM HH:mm:ss.fff} --- {text} ---");
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
                    return ConsoleColor.Red;
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
