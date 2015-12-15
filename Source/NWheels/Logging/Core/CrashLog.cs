using System;
using System.IO;
using NWheels.Utilities;

namespace NWheels.Logging.Core
{
    public static class CrashLog
    {
        public static void RegisterUnhandledExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void UnregisterUnhandledExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionHandler;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LogUnhandledException(object exceptionObject, bool isTerminating)
        {
            var message =
                Environment.NewLine +
                Environment.NewLine +
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") +
                " > UNHANDLED EXCEPTION! " +
                (isTerminating ? "APP DOMAIN WILL TERMINATE! " : "(will not terminate app domain) ") +
                Environment.NewLine +
                (exceptionObject != null ? exceptionObject.ToString() : "(no exception available)") + 
                Environment.NewLine + 
                Environment.NewLine;

            var filePath = PathUtility.HostBinPath("crash.log");
            File.AppendAllText(filePath, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            LogUnhandledException(e.ExceptionObject, e.IsTerminating);
        }
    }
}
