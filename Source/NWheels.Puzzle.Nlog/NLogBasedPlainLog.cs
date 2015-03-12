using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NWheels.Core.Hosting;
using NWheels.Core.Logging;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Utilities;
using LogLevel = NLog.LogLevel;

namespace NWheels.Puzzle.Nlog
{
    public class NLogBasedPlainLog : IPlainLog
    {
        public const string PlainTextLoggerName = "PlainText";
        public const string NameValuePairLoggerName = "NameValuePairs";
        public const string PlainTextFileTargetName = "PlainTextFile";
        public const string PlainTextConsoleTargetName = "PlainTextConsole";
        public const string PlainTextEventLogTargetName = "PlainTextEventLog";
        public const string NameValuePairFileTargetName = "NameValuePairsFile";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly Logger _plainTextLogger;
        private readonly Logger _nameValuePairLogger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private NLogBasedPlainLog()
        {
            var config = new LoggingConfiguration();

            ConfigureTextFileOutput(config);
            ConfigureNameValuePairOutput(config);

            LogManager.Configuration = config;

            _plainTextLogger = LogManager.GetLogger(PlainTextLoggerName);
            _nameValuePairLogger = LogManager.GetLogger(NameValuePairLoggerName);
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

            LogManager.Configuration.AddTarget(PlainTextConsoleTargetName, consoleTarget);

            var consoleRule = new LoggingRule(PlainTextLoggerName, LogLevel.Trace, consoleTarget);

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
                    _plainTextLogger.Debug(node.SingleLineText);
                    break;
                case NWheels.Logging.LogLevel.Info:
                    _plainTextLogger.Info(node.SingleLineText);
                    _nameValuePairLogger.Info(node.NameValuePairsText);
                    break;
                case NWheels.Logging.LogLevel.Warning:
                    _plainTextLogger.Warn(node.SingleLineText, node.Exception);
                    _nameValuePairLogger.Warn(node.NameValuePairsText);
                    break;
                case NWheels.Logging.LogLevel.Error:
                    _plainTextLogger.Error(node.SingleLineText, node.Exception);
                    _nameValuePairLogger.Error(node.NameValuePairsText);
                    break;
                case NWheels.Logging.LogLevel.Critical:
                    _plainTextLogger.Fatal(node.SingleLineText, node.Exception);
                    _nameValuePairLogger.Fatal(node.NameValuePairsText);
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LogActivity(NWheels.Logging.ActivityLogNode activity)
        {
            if ( activity.Parent != null )
            {
                _plainTextLogger.Trace(activity.SingleLineText);
            }
            else
            {
                _plainTextLogger.Trace("[THREAD:{0}] {1}", activity.TaskType, activity.SingleLineText);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Debug(string format, params object[] args)
        {
            _plainTextLogger.Debug(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Info(string format, params object[] args)
        {
            _plainTextLogger.Info(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Warning(string format, params object[] args)
        {
            _plainTextLogger.Warn(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Error(string format, params object[] args)
        {
            _plainTextLogger.Error(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Critical(string format, params object[] args)
        {
            _plainTextLogger.Fatal(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureTextFileOutput(LoggingConfiguration config)
        {
            var target = new FileTarget() {
                Name = PlainTextFileTargetName,
                FileName = PathUtility.LocalBinPath("..\\Logs\\PlainLog", "plain.log"),
                CreateDirs = true,
                ArchiveEvery = FileArchivePeriod.Hour,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ArchiveFileName = PathUtility.LocalBinPath("..\\Logs\\PlainLog", @"${date:universalTime=True:format=yyyyMMdd}-{####}.plain.log"),
                MaxArchiveFiles = 10,
                EnableFileDelete = true,
                ConcurrentWrites = false,
                KeepFileOpen = false,
            };

            target.Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss.fff}|${level:uppercase=true}|${message}|${exception:format=ToString}";
            config.AddTarget(PlainTextFileTargetName, target);

            var plainTextFileRule = new LoggingRule(PlainTextLoggerName, LogLevel.Trace, target);
            config.LoggingRules.Add(plainTextFileRule);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureNameValuePairOutput(LoggingConfiguration config)
        {
            var target = new FileTarget() {
                Name = NameValuePairFileTargetName,
                FileName = PathUtility.LocalBinPath("..\\Logs\\PlainLog", "nvp.log"),
                CreateDirs = true,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ArchiveFileName = PathUtility.LocalBinPath("..\\Logs\\PlainLog", @"${date:universalTime=True:format=yyyyMMdd}-{####}.nvp.log"),
                MaxArchiveFiles = 10,
                EnableFileDelete = true,
                ConcurrentWrites = false,
                KeepFileOpen = false,
            };

            target.Layout = @"${message}";

            config.AddTarget(NameValuePairFileTargetName, target);

            var rule = new LoggingRule(NameValuePairLoggerName, LogLevel.Info, target);
            config.LoggingRules.Add(rule);
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
