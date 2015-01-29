using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Core.Hosting;
using NWheels.Core.Logging;
using NWheels.Hosting;
using NWheels.Puzzle.Nlog;
using NWheels.Utilities;

namespace NWheels.Hosts.Service
{
    static class Program
    {
        private static NodeHostConfig s_NodeHostConfig;
        private static IPlainLog s_Log;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main()
        {
            CrashLog.RegisterUnhandledExceptionHandler();

            s_Log = NLogBasedPlainLog.Instance;
            s_Log.Info("NWheels Windows Service Host version {0}", typeof(Program).Assembly.GetName().Version);

            try
            {
                LoadNodeHostConfig();
                s_Log.ConfigureWindowsEventLogOutput(logName: "Application", sourceName: s_NodeHostConfig.ApplicationName + "." + s_NodeHostConfig.NodeName);
            }
            catch ( Exception e )
            {
                s_Log.Critical("FAILED TO LOAD {0}: {1}", NodeHostConfig.DefaultFileName, e.Message);
                return 1;
            }

            var servicesToRun = new ServiceBase[] { 
                new WindowsService() 
            };

            ServiceBase.Run(servicesToRun);
            return 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void LoadNodeHostConfig()
        {
            s_Log.Debug("Loading {0}", NodeHostConfig.DefaultFileName);

            s_NodeHostConfig = NodeHostConfig.LoadFromFile(PathUtility.LocalBinPath(NodeHostConfig.DefaultFileName));
            s_NodeHostConfig.Validate();

            s_Log.Info(s_NodeHostConfig.ToLogString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static NodeHostConfig HostConfig
        {
            get { return s_NodeHostConfig; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IPlainLog Log
        {
            get { return s_Log; }
        }
    }
}
