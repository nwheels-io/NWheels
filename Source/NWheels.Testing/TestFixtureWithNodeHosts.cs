using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using NUnit.Framework;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Testing.Entities.Impl;

namespace NWheels.Testing
{
    [TestFixture]
    public abstract class TestFixtureWithNodeHosts
    {


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConsolePlainLog : IPlainLog
        {
            private readonly string _loggerName;
            private readonly LogLevel _logLevel;
            private readonly Stopwatch _clock;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ConsolePlainLog(string loggerName, LogLevel logLevel, Stopwatch clock)
            {
                _loggerName = loggerName;
                _logLevel = logLevel;
                _clock = clock;
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
                        Error(node.SingleLineText + " + EXCEPTION [" + node.Exception.Message + "]");
                        break;
                    case NWheels.Logging.LogLevel.Critical:
                        Critical(node.SingleLineText + " + EXCEPTION [" + node.Exception.Message + "]");
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
                    Console.WriteLine(GetLogPrefix() + ">debug> " + format, args);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Trace(string format, params object[] args)
            {
                if ( _logLevel <= LogLevel.Verbose )
                {
                    Console.WriteLine(GetLogPrefix() + ">trace> " + format, args);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Info(string format, params object[] args)
            {
                if ( _logLevel <= LogLevel.Info )
                {
                    Console.WriteLine(GetLogPrefix() + ">INFO > " + format, args);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Warning(string format, params object[] args)
            {
                if ( _logLevel <= LogLevel.Warning )
                {
                    Console.WriteLine(GetLogPrefix() + ">WARNI> " + format, args);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Error(string format, params object[] args)
            {
                if ( _logLevel <= LogLevel.Error )
                {
                    Console.WriteLine(GetLogPrefix() + ">ERROR> " + format, args);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Critical(string format, params object[] args)
            {
                if ( _logLevel <= LogLevel.Critical )
                {
                    Console.WriteLine(GetLogPrefix() + ">CRITI> " + format, args);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private string GetLogPrefix()
            {
                return string.Format("+{0} >{1}", _clock.Elapsed, _loggerName);
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
}
