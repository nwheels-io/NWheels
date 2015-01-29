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
using NWheels.Core.Logging;
using NWheels.Logging;
using NWheels.Utilities;
using LogLevel = NLog.LogLevel;

namespace NWheels.Puzzle.Nlog
{
    public class NLogBasedPlainLog : IPlainLog
    {
        private readonly Logger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private NLogBasedPlainLog()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget() {
                FileName = PathUtility.LocalBinPath("nwheels.log"),
            };

            fileTarget.Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss.fff}|${level:uppercase=true}|${message}|${exception:format=ToString}";

            config.AddTarget("File", fileTarget);

            var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);

            config.LoggingRules.Add(fileRule);

            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ConfigureConsoleOutput()
        {
            var consoleTarget = new ColoredConsoleTarget();

            consoleTarget.Layout = @"${date:universalTime=true:format=HH\:mm\:ss.fff} ${message} ${exception:format=Message}";
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

        public void ConfigureWindowsEventLogOutput(string logName, string sourceName)
        {
            var eventLogTarget = new EventLogTarget();

            eventLogTarget.MachineName = ".";
            eventLogTarget.Log = logName;
            eventLogTarget.Source = sourceName;

            if ( !eventLogTarget.IsInstalled(null).GetValueOrDefault() )
            {
                eventLogTarget.Install(new InstallationContext());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LogNode(NWheels.Logging.LogNode node)
        {
            switch ( node.Level )
            {
                case NWheels.Logging.LogLevel.Debug:
                case NWheels.Logging.LogLevel.Verbose:
                    _logger.Debug(node.SingleLineText);
                    break;
                case NWheels.Logging.LogLevel.Info:
                    _logger.Info(node.SingleLineText);
                    break;
                case NWheels.Logging.LogLevel.Warning:
                    _logger.Warn(node.SingleLineText, node.Exception);
                    break;
                case NWheels.Logging.LogLevel.Error:
                    _logger.Error(node.SingleLineText, node.Exception);
                    break;
                case NWheels.Logging.LogLevel.Critical:
                    _logger.Fatal(node.SingleLineText, node.Exception);
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LogActivity(NWheels.Logging.ActivityLogNode activity)
        {
            if ( activity.Parent != null )
            {
                _logger.Trace(activity.SingleLineText);
            }
            else
            {
                _logger.Trace("[THREAD:{0}] {1}", activity.TaskType, activity.SingleLineText);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Debug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Info(string format, params object[] args)
        {
            _logger.Info(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Warning(string format, params object[] args)
        {
            _logger.Warn(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Error(string format, params object[] args)
        {
            _logger.Error(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Critical(string format, params object[] args)
        {
            _logger.Fatal(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly NLogBasedPlainLog s_Instance;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static NLogBasedPlainLog()
        {
            s_Instance = new NLogBasedPlainLog();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static NLogBasedPlainLog Instance
        {
            get { return s_Instance; }
        }

    }
}
