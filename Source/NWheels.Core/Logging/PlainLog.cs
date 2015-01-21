using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NWheels.Logging;
using NWheels.Utilities;
using LogLevel = NLog.LogLevel;

namespace NWheels.Core.Logging
{
    public static class PlainLog
    {
        private static Logger s_Logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static PlainLog()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget() {
                FileName = PathUtility.LocalBinPath("nwheels.log"),
            };

            fileTarget.Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss.fff}|${level:uppercase=true}|${message}";

            config.AddTarget("File", fileTarget);

            var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);

            config.LoggingRules.Add(fileRule);

            LogManager.Configuration = config;
            s_Logger = LogManager.GetCurrentClassLogger();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ConfigureConsoleOutput()
        {
            var consoleTarget = new ColoredConsoleTarget();

            consoleTarget.Layout = @"${date:format=HH\:mm\:ss.fff} ${message}";
            consoleTarget.UseDefaultRowHighlightingRules = false;
            consoleTarget.RowHighlightingRules.Clear();
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Trace and starts-with('${message}','[THREAD:')", ConsoleOutputColor.Cyan, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Trace", ConsoleOutputColor.DarkCyan, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Debug", ConsoleOutputColor.DarkGray, ConsoleOutputColor.Black));
            
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Info", ConsoleOutputColor.White, ConsoleOutputColor.Black));

            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level >= LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.Black));

            LogManager.Configuration.AddTarget("Console", consoleTarget);

            var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);

            LogManager.Configuration.LoggingRules.Add(consoleRule);
            LogManager.ReconfigExistingLoggers();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LogNode(NWheels.Logging.LogNode node)
        {
            switch ( node.Level )
            {
                case NWheels.Logging.LogLevel.Debug:
                case NWheels.Logging.LogLevel.Verbose:
                    s_Logger.Debug(node.SingleLineText);
                    break;
                case NWheels.Logging.LogLevel.Info:
                    s_Logger.Info(node.SingleLineText);
                    break;
                case NWheels.Logging.LogLevel.Warning:
                    s_Logger.Warn(node.SingleLineText, node.Exception);
                    break;
                case NWheels.Logging.LogLevel.Error:
                    s_Logger.Error(node.SingleLineText, node.Exception);
                    break;
                case NWheels.Logging.LogLevel.Critical:
                    s_Logger.Fatal(node.SingleLineText, node.Exception);
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LogActivity(NWheels.Logging.ActivityLogNode activity)
        {
            if ( activity.Parent != null )
            {
                s_Logger.Trace(activity.SingleLineText);
            }
            else
            {
                s_Logger.Trace("[THREAD:{0}] {1}", activity.TaskType, activity.SingleLineText);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Debug(string format, params object[] args)
        {
            s_Logger.Debug(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Info(string format, params object[] args)
        {
            s_Logger.Info(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Warning(string format, params object[] args)
        {
            s_Logger.Warn(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Error(string format, params object[] args)
        {
            s_Logger.Error(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Critical(string format, params object[] args)
        {
            s_Logger.Fatal(format, args);
        }
    }
}
