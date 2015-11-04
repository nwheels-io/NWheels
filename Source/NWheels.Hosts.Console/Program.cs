using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Autofac;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging.Core;
using NWheels.Stacks.Nlog;
using NWheels.Utilities;

namespace NWheels.Hosts.Console
{
    class Program
    {
        private static BootConfiguration _s_bootConfig;
        private static NodeHost _s_nodeHost;
        private static ManualResetEvent _s_stopRequested;
        private static IPlainLog _s_log;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static int Main(string[] args)
        {
            CrashLog.RegisterUnhandledExceptionHandler();

            _s_log = NLogBasedPlainLog.Instance;
            _s_log.ConfigureConsoleOutput();
            
            _s_log.Info("NWheels Console Host version {0}", typeof(Program).Assembly.GetName().Version);

            try
            {
                LoadBootConfig(args);
                System.Console.Title += ": " + _s_bootConfig.ApplicationName + "." + _s_bootConfig.NodeName;
            }
            catch ( Exception e )
            {
                _s_log.Critical("FAILED TO LOAD BOOT CONFIG: {0}", e.Message);
                return 1;
            }

            try
            {
                StartNodeHost();
            }
            catch ( Exception e )
            {
                _s_log.Critical("NODE FAILED TO START! {0}", e.ToString());
                return 2;
            }

            BlockUntilStopRequested();

            try
            {
                StopNodeHost();
            }
            catch ( Exception e )
            {
                _s_log.Warning("NODE WAS NOT CORRECTLY STOPPED! {0}", e.ToString());
            }

            return 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void LoadBootConfig(string[] programArgs)
        {
            var configFilePath = PathUtility.HostBinPath(programArgs.Length > 0 ? programArgs[0] : BootConfiguration.DefaultBootConfigFileName);

            _s_log.Debug("Loading configuration from: {0}", configFilePath);

            _s_bootConfig = BootConfiguration.LoadFromFile(configFilePath);
            _s_bootConfig.Validate();

            _s_log.Debug("> Application Name   - {0}", _s_bootConfig.ApplicationName);
            _s_log.Debug("> Node Name          - {0}", _s_bootConfig.NodeName);

            foreach ( var module in _s_bootConfig.FrameworkModules )
            {
                _s_log.Debug("> Framework Module   - {0}", module.Name);
            }

            foreach ( var module in _s_bootConfig.ApplicationModules )
            {
                _s_log.Debug("> Application Module - {0}", module.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static void StartNodeHost()
        {
            _s_nodeHost = new NodeHost(_s_bootConfig, RegisterHostComponents);
            _s_nodeHost.LoadAndActivate();

            _s_stopRequested = new ManualResetEvent(initialState: false);
            System.Console.CancelKeyPress += Console_CancelKeyPress;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void RegisterHostComponents(ContainerBuilder builder)
        {
            builder.RegisterModule<NWheels.Stacks.Nlog.ModuleLoader>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void StopNodeHost()
        {
            _s_nodeHost.DeactivateAndUnload();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void BlockUntilStopRequested()
        {
            var stopRequestFilePath = PathUtility.HostBinPath(fileName: "stop.request");

            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine();
            System.Console.WriteLine("{0:HH:mm:ss.fff} UP AND RUNNING. PRESS CTRL + BREAK TO STOP", DateTime.UtcNow);
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Gray;

            if ( File.Exists(stopRequestFilePath) )
            {
                File.Delete(stopRequestFilePath);
            }

            var stopRequestPollingTimer = new Timer(state => {
                if ( File.Exists(stopRequestFilePath) )
                {
                    _s_stopRequested.Set();
                }
            }, state:null, dueTime: 1000, period: 1000);

            _s_stopRequested.WaitOne();

            stopRequestPollingTimer.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _s_stopRequested.Set();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static void GenerateDummyNodeHostConfig()
        {
            _s_bootConfig = new BootConfiguration() {
                ApplicationName = "Test App",
                NodeName = "Test Node",
                ApplicationModules = new List<BootConfiguration.ModuleConfig> {
                    new BootConfiguration.ModuleConfig {
                        Assembly = "Dummy.Module"
                    }
                }
            };

            var serializer = new DataContractSerializer(typeof(BootConfiguration));
            using ( var file = File.Create(PathUtility.HostBinPath(BootConfiguration.DefaultBootConfigFileName)) )
            {
                serializer.WriteObject(file, _s_bootConfig);
                file.Flush();
            }

            _s_bootConfig = null;
        }
    }
}
