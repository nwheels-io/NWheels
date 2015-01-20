using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using NWheels.Utilities;

namespace NWheels.Core.Logging
{
    public static class PlainLog
    {
        private static Logger s_Logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static PlainLog()
        {
            var fileTarget = new FileTarget() {
                FileName = PathUtility.LocalBinPath("nwheels.log")
            };

            LogManager.Configuration.AddTarget("File", fileTarget);

            var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);

            LogManager.Configuration.LoggingRules.Add(fileRule);

            s_Logger = LogManager.GetCurrentClassLogger();
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

        public static void LogDebug(string message)
        {
            s_Logger.Debug(message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LogInfo(string message)
        {
            s_Logger.Info(message);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LogWarning(string message)
        {
            s_Logger.Warn(message);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LogError(string message)
        {
            s_Logger.Error(message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LogCritical(string message)
        {
            s_Logger.Fatal(message);
        }
    }
}
