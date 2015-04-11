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

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var message =
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + 
                " > UNHANDLED EXCEPTION! " + 
                (e.IsTerminating ? "APP DOMAIN WILL TERMINATE! " : "") +
                (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "(no exception available)");

            File.WriteAllText(
                PathUtility.LocalBinPath("crash.log"), 
                "UNHANDLED EXCEPTION! " + e.ExceptionObject.ToString());
        }
    }
}
