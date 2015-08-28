using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Modules.Output;
using NWheels.Logging;
using NWheels.Logging.Core;

namespace NWheels.Tools.TestBoard.Services
{
    [Export(typeof(IPlainLog))]
    public class OutputPlainLogService : IPlainLog
    {
        private readonly IOutput _output;
        private readonly string _loggerName;
        private readonly LogLevel _logLevel;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OutputPlainLogService(IOutput output, string loggerName, LogLevel logLevel)
        {
            _output = output;
            _loggerName = loggerName;
            _logLevel = logLevel;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public OutputPlainLogService(IOutput output) 
            : this(output, "Output", LogLevel.Info)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ConfigureConsoleOutput()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ConfigureWindowsEventLogOutput(string logName, string sourceName)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LogNode(NWheels.Logging.LogNode node)
        {
            switch ( node.Level )
            {
                case NWheels.Logging.LogLevel.Debug:
                case NWheels.Logging.LogLevel.Verbose:
                    Debug(node.SingleLineText);
                    break;
                case NWheels.Logging.LogLevel.Info:
                    Info(node.SingleLineText);
                    break;
                case NWheels.Logging.LogLevel.Warning:
                    Warning(node.SingleLineText + AddExceptionIf(node.Exception));
                    break;
                case NWheels.Logging.LogLevel.Error:
                    Error(node.SingleLineText + AddExceptionIf(node.Exception));
                    break;
                case NWheels.Logging.LogLevel.Critical:
                    Critical(node.SingleLineText + AddExceptionIf(node.Exception));
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LogActivity(NWheels.Logging.ActivityLogNode activity)
        {
            if ( activity.Parent != null )
            {
                Trace(activity.SingleLineText);
            }
            else
            {
                Trace("[THREAD:{0}] {1}", activity.TaskType, activity.SingleLineText);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Debug(string format, params object[] args)
        {
            if ( _logLevel == LogLevel.Debug )
            {
                _output.AppendLine(string.Format(GetLogPrefix() + ">debug> " + format, args));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Trace(string format, params object[] args)
        {
            if ( _logLevel <= LogLevel.Verbose )
            {
                _output.AppendLine(string.Format(GetLogPrefix() + ">trace> " + format, args));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Info(string format, params object[] args)
        {
            if ( _logLevel <= LogLevel.Info )
            {
                _output.AppendLine(string.Format(GetLogPrefix() + ">INFO > " + format, args));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Warning(string format, params object[] args)
        {
            if ( _logLevel <= LogLevel.Warning )
            {
                _output.AppendLine(string.Format(GetLogPrefix() + ">WARNI> " + format, args));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Error(string format, params object[] args)
        {
            if ( _logLevel <= LogLevel.Error )
            {
                _output.AppendLine(string.Format(GetLogPrefix() + ">ERROR> " + format, args));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Critical(string format, params object[] args)
        {
            if ( _logLevel <= LogLevel.Critical )
            {
                _output.AppendLine(string.Format(GetLogPrefix() + ">CRITI> " + format, args));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetLogPrefix()
        {
            return _loggerName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string AddExceptionIf(Exception exception)
        {
            if ( exception != null )
            {
                return " + EXCEPTION [" + exception.Message + "]";
            }
            else
            {
                return "";
            }
        }
    }
}
