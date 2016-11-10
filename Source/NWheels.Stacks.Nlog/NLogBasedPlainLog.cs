using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Utilities;
using NWheels.Extensions;
using LogLevel = NLog.LogLevel;
using NWheelsLogLevel = NWheels.Logging.LogLevel;

namespace NWheels.Stacks.Nlog
{
    public class NLogBasedPlainLog : LifecycleEventListenerBase, IPlainLog
    {
        public static readonly string BootTextLoggerName = "BootText";
        public static readonly string PlainTextLoggerName = "PlainText";
        public static readonly string NameValuePairLoggerName = "NameValuePairs";
        public static readonly string BootTextFileTargetName = "BootTextFile";
        public static readonly string BootErrorTextFileTargetName = "BootErrorTextFile";
        public static readonly string PlainTextFileTargetName = "PlainTextFile";
        public static readonly string PlainErrorTextFileTargetName = "PlainErrorTextFile";
        public static readonly string PlainTextConsoleTargetName = "PlainTextConsole";
        public static readonly string PlainTextEventLogTargetName = "PlainTextEventLog";
        public static readonly string NameValuePairFileTargetName = "NameValuePairsFile";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_activityStartPrefix = "<BEGAN>";
        private static readonly string _s_activityEndPrefix = "<ENDED>";
        private static readonly string _s_activityEndSuffixFormat = "[METRICS: duration={0:#,##0}ms ; db.roundtrips = {1} ; db.duration = {2:#,##0}ms]";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly Guid _bootCorrelationId;
        private readonly string _bootLogFolder;
        private readonly Logger _bootTextLogger;
        private readonly Action<ActivityLogNode> _printClosedActivityDelegate;
        private NWheelsLogLevel _currentLogLevel;
        private Logger _currentTextLogger;
        private INodeConfiguration _currentNode;
        private IFrameworkLoggingConfiguration _frameworkConfig;

        //private readonly Logger _nameValuePairLogger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private NLogBasedPlainLog()
        {
            _bootCorrelationId = Guid.NewGuid();
            _bootLogFolder = PathUtility.HostBinPath("..\\Logs\\BootLog");
            
            var config = new LoggingConfiguration();
            ConfigureBootLoggerTarget(config);
            //ConfigureNameValuePairOutput(config);
            LogManager.Configuration = config;

            _bootTextLogger = LogManager.GetLogger(BootTextLoggerName);
            _currentTextLogger = _bootTextLogger; // this will change in NodeConfigured()
            _currentLogLevel = NWheelsLogLevel.Debug;

            _printClosedActivityDelegate = this.PrintClosedActivity;
            //_nameValuePairLogger = LogManager.GetLogger(NameValuePairLoggerName);
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

            var consoleRule1 = new LoggingRule(PlainTextLoggerName, LogLevel.Info, consoleTarget);
            var consoleRule2 = new LoggingRule(BootTextLoggerName, LogLevel.Info, consoleTarget);

            LogManager.Configuration.LoggingRules.Add(consoleRule1);
            LogManager.Configuration.LoggingRules.Add(consoleRule2);
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
            LogNode(node, prefix: string.Empty, suffix: string.Empty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LogNode(NWheels.Logging.LogNode node, string prefix, string suffix)
        {
            switch ( node.Level )
            {
                case NWheelsLogLevel.Debug:
                case NWheelsLogLevel.Verbose:
                    if (_currentLogLevel <= NWheelsLogLevel.Verbose)
                    {
                        _currentTextLogger.Debug(prefix + node.SingleLineText + suffix);
                    }
                    break;
                case NWheelsLogLevel.Info:
                    if (_currentLogLevel <= NWheelsLogLevel.Info)
                    {
                        _currentTextLogger.Info(prefix + node.SingleLineText + suffix);
                        //_nameValuePairLogger.Info(node.NameValuePairsText);
                    }
                    break;
                case NWheelsLogLevel.Warning:
                    if (_currentLogLevel <= NWheelsLogLevel.Warning)
                    {
                        _currentTextLogger.Warn(node.Exception, prefix + node.SingleLineText + suffix);
                        //_nameValuePairLogger.Warn(node.NameValuePairsText);
                    }
                    break;
                case NWheelsLogLevel.Error:
                    if (_currentLogLevel <= NWheelsLogLevel.Error)
                    {
                        _currentTextLogger.Error(node.Exception, prefix + node.SingleLineText + suffix);
                        //_nameValuePairLogger.Error(node.NameValuePairsText);
                    }
                    break;
                case NWheelsLogLevel.Critical:
                    if (_currentLogLevel <= NWheelsLogLevel.Critical)
                    {
                        _currentTextLogger.Fatal(node.Exception, prefix + node.SingleLineText + suffix);
                        //_nameValuePairLogger.Fatal(node.NameValuePairsText);
                    }
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LogActivity(NWheels.Logging.ActivityLogNode activity)
        {
            if (activity.Parent == null)
            {
                _currentTextLogger.Trace("[THREAD:{0}] {1}", activity.TaskType, activity.SingleLineText);
            }
            
            if (activity.Level >= _currentLogLevel)
            {
                _currentTextLogger.Trace(_s_activityStartPrefix + activity.SingleLineText);
                activity.Closed += _printClosedActivityDelegate;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Debug(string format, params object[] args)
        {
            _currentTextLogger.Debug(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Info(string format, params object[] args)
        {
            _currentTextLogger.Info(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Warning(string format, params object[] args)
        {
            _currentTextLogger.Warn(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Error(string format, params object[] args)
        {
            _currentTextLogger.Error(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Critical(string format, params object[] args)
        {
            _currentTextLogger.Fatal(format, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        public override void InjectDependencies(IComponentContext components)
        {
            _currentNode = components.Resolve<INodeConfiguration>();
            _frameworkConfig = components.Resolve<IFrameworkLoggingConfiguration>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeConfigured(List<ILifecycleEventListener> additionalComponentsToHost)
        {
            _currentLogLevel = _frameworkConfig.Level;

            ConfigurePlainLoggerTarget(LogManager.Configuration);
            LogManager.ReconfigExistingLoggers();

            _currentTextLogger = LogManager.GetLogger(PlainTextLoggerName);
            _currentTextLogger.Info("=== CONTINUED FROM BOOT LOG CORRELATION ID=[{0:N}], LOCATION: {1} ===", _bootCorrelationId, _bootLogFolder);

            var bootConfig = _currentNode as BootConfiguration;
            if (bootConfig != null)
            {
                _currentTextLogger.Info(bootConfig.ToLogString());
            }

            _currentTextLogger.Debug("continue logging, at NLogBasedPlainLog.NodeConfigured");
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureBootLoggerTarget(LoggingConfiguration config)
        {
            if ( !Directory.Exists(_bootLogFolder) )
            {
                Directory.CreateDirectory(_bootLogFolder);
            }

            var target1 = new FileTarget() {
                Name = BootTextFileTargetName,
                FileName = Path.Combine(_bootLogFolder, @"${machinename}.boot.log"),
                CreateDirs = true,
                ArchiveEvery = FileArchivePeriod.Hour,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ArchiveFileName = Path.Combine(_bootLogFolder, @"${machinename}-${date:universalTime=True:format=yyyyMMdd}-{####}.boot.log"),
                MaxArchiveFiles = 10,
                EnableFileDelete = true,
                ConcurrentWrites = false,
                KeepFileOpen = false,
            };

            target1.Layout = @"${date:universalTime=True:format=yyyy-MM-dd HH\:mm\:ss.fff}|${level:uppercase=true}|${message}|${exception:format=ToString}";
            config.AddTarget(PlainTextFileTargetName, target1);

            var bootTextFileRule = new LoggingRule(BootTextLoggerName, LogLevel.Debug, target1);
            config.LoggingRules.Add(bootTextFileRule);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigurePlainLoggerTarget(LoggingConfiguration config)
        {
            var logFolderSuffix = string.Format(
                ".{0}.{1}{2}",
                _currentNode.ApplicationName,
                _currentNode.NodeName,
                string.IsNullOrEmpty(_currentNode.InstanceId) ? string.Empty : "." + _currentNode.InstanceId);

            var logFolder = PathUtility.HostBinPath((_frameworkConfig.PlainLogFolder ?? "..\\Logs\\PlainLog") + logFolderSuffix);

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            _bootTextLogger.Info("=== END OF BOOT LOG. PLAIN LOG CORRELATION ID=[{0:N}], LOCATION: {1} ===", _bootCorrelationId, logFolder);
            
            var target1 = new FileTarget() {
                Name = PlainTextFileTargetName,
                FileName = Path.Combine(logFolder, @"all-${machinename}.plain.log"),
                CreateDirs = true,
                ArchiveEvery = FileArchivePeriod.Hour,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ArchiveFileName = Path.Combine(logFolder, @"all-${machinename}-${date:universalTime=True:format=yyyyMMdd}-{####}.plain.log"),
                MaxArchiveFiles = 10,
                EnableFileDelete = true,
                ConcurrentWrites = false,
                KeepFileOpen = false,
            };

            var target2 = new FileTarget() {
                Name = PlainTextFileTargetName,
                FileName = Path.Combine(logFolder, @"err-${machinename}.plain.error.log"),
                CreateDirs = true,
                ArchiveEvery = FileArchivePeriod.Hour,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ArchiveFileName = Path.Combine(logFolder, @"err-${machinename}-${date:universalTime=True:format=yyyyMMdd}-{####}.plain.error.log"),
                MaxArchiveFiles = 10,
                EnableFileDelete = true,
                ConcurrentWrites = false,
                KeepFileOpen = false,
            };

            target1.Layout = @"${date:universalTime=True:format=yyyy-MM-dd HH\:mm\:ss.fff}|${level:uppercase=true}|${message}|${exception:format=ShortType,Message}";
            config.AddTarget(PlainTextFileTargetName, target1);

            target2.Layout = @"${date:universalTime=True:format=yyyy-MM-dd HH\:mm\:ss.fff}|${level:uppercase=true}|${message}|${exception:format=ToString}";
            config.AddTarget(PlainTextFileTargetName, target2);

            var plainTextFileRule1 = new LoggingRule(PlainTextLoggerName, ToNLogLevel(_frameworkConfig.Level), target1);
            config.LoggingRules.Add(plainTextFileRule1);

            var plainTextFileRule2 = new LoggingRule(PlainTextLoggerName, LogLevel.Warn, target2);
            config.LoggingRules.Add(plainTextFileRule2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PrintClosedActivity(ActivityLogNode activity)
        {
            LogNode(
                activity, 
                prefix: _s_activityEndPrefix,
                suffix: string.Format(
                    _s_activityEndSuffixFormat,
                    activity.MillisecondsDuration, activity.DbTotal.Count, activity.DbTotal.MicrosecondsDuration / 1000
                )
            );
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private void ConfigureNameValuePairOutput(LoggingConfiguration config)
        //{
        //    var target = new FileTarget() {
        //        Name = NameValuePairFileTargetName,
        //        FileName = PathUtility.HostBinPath("..\\Logs\\PlainLog", @"${machinename}.nvp.log"),
        //        CreateDirs = true,
        //        ArchiveEvery = FileArchivePeriod.Day,
        //        ArchiveNumbering = ArchiveNumberingMode.Sequence,
        //        ArchiveFileName = PathUtility.HostBinPath("..\\Logs\\PlainLog", @"${machinename}-${date:universalTime=True:format=yyyyMMdd}-{####}.nvp.log"),
        //        MaxArchiveFiles = 10,
        //        EnableFileDelete = true,
        //        ConcurrentWrites = false,
        //        KeepFileOpen = false,
        //    };

        //    target.Layout = @"${message}";

        //    config.AddTarget(NameValuePairFileTargetName, target);

        //    var rule = new LoggingRule(NameValuePairLoggerName, LogLevel.Info, target);
        //    config.LoggingRules.Add(rule);
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private LogLevel ToNLogLevel(NWheelsLogLevel value)
        {
            switch (value)
            {
                case NWheelsLogLevel.Debug:
                case NWheelsLogLevel.Verbose:
                    return LogLevel.Debug;
                case NWheelsLogLevel.Warning:
                    return LogLevel.Warn;
                case NWheelsLogLevel.Error:
                    return LogLevel.Error;
                case NWheelsLogLevel.Critical:
                    return LogLevel.Fatal;
                default:
                    return LogLevel.Info;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly NLogBasedPlainLog _s_instance;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static NLogBasedPlainLog()
        {
            _s_instance = new NLogBasedPlainLog();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static NLogBasedPlainLog Instance
        {
            get { return _s_instance; }
        }
    }
}
