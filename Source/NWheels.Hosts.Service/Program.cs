using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Hosting;
using NWheels.Logging.Core;
using NWheels.Stacks.Nlog;
using NWheels.Utilities;

namespace NWheels.Hosts.Service
{
    static class Program
    {
        private static BootConfiguration s_BootConfig;
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
                s_Log.ConfigureWindowsEventLogOutput(logName: "Application", sourceName: s_BootConfig.ApplicationName + "." + s_BootConfig.NodeName);
            }
            catch ( Exception e )
            {
                s_Log.Critical("FAILED TO LOAD {0}: {1}", BootConfiguration.DefaultBootConfigFileName, e.Message);
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
            s_Log.Debug("Loading {0}", BootConfiguration.DefaultBootConfigFileName);

            s_BootConfig = BootConfiguration.LoadFromFile(PathUtility.HostBinPath(BootConfiguration.DefaultBootConfigFileName));
            s_BootConfig.Validate();

            s_Log.Info(s_BootConfig.ToLogString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static BootConfiguration HostConfig
        {
            get { return s_BootConfig; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IPlainLog Log
        {
            get { return s_Log; }
        }
    }
}
