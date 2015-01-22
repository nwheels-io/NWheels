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
using NWheels.Core.Hosting;
using NWheels.Core.Logging;
using NWheels.Hosting;
using NWheels.Puzzle.Nlog;
using NWheels.Utilities;

namespace NWheels.Hosts.Console
{
    class Program
    {
        private static NodeHostConfig s_NodeHostConfig;
        private static NodeHost s_NodeHost;
        private static ManualResetEvent s_StopRequested;
        private static IPlainLog s_Log;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static int Main(string[] args)
        {
            //GenerateDummyNodeHostConfig();
            CrashLog.RegisterUnhandledExceptionHandler();

            s_Log = NLogBasedPlainLog.Instance;
            s_Log.ConfigureConsoleOutput();
            
            s_Log.Info("NWheels Console Host version {0}", typeof(Program).Assembly.GetName().Version);

            try
            {
                LoadNodeHostConfig();
            }
            catch ( Exception e )
            {
                s_Log.Critical("FAILED TO LOAD {0}: {1}", NodeHostConfig.DefaultFileName, e.Message);
                return 1;
            }

            try
            {
                StartNodeHost();
            }
            catch ( Exception e )
            {
                s_Log.Critical("NODE FAILED TO START! {0}", e.ToString());
                return 2;
            }

            WaitUntilStopRequested();

            try
            {
                StopNodeHost();
            }
            catch ( Exception e )
            {
                s_Log.Warning("NODE WAS NOT CORRECTLY STOPPED! {0}", e.ToString());
            }

            return 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void LoadNodeHostConfig()
        {
            s_Log.Debug("Loading {0}", NodeHostConfig.DefaultFileName);

            s_NodeHostConfig = NodeHostConfig.LoadFromFile(PathUtility.LocalBinPath(NodeHostConfig.DefaultFileName));
            s_NodeHostConfig.Validate();

            s_Log.Debug("> Application Name   - {0}", s_NodeHostConfig.ApplicationName);
            s_Log.Debug("> Node Name          - {0}", s_NodeHostConfig.NodeName);

            foreach ( var moduleString in s_NodeHostConfig.FrameworkModules )
            {
                s_Log.Debug("> Framework Module   - {0}", moduleString);
            }

            foreach ( var moduleString in s_NodeHostConfig.ApplicationModules )
            {
                s_Log.Debug("> Application Module - {0}", moduleString);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static void StartNodeHost()
        {
            s_NodeHost = new NodeHost(s_NodeHostConfig, RegisterHostComponents);
            s_NodeHost.LoadAndActivate();

            s_StopRequested = new ManualResetEvent(initialState: false);
            System.Console.CancelKeyPress += Console_CancelKeyPress;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void RegisterHostComponents(ContainerBuilder builder)
        {
            builder.RegisterModule<NWheels.Puzzle.Nlog.ModuleLoader>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void StopNodeHost()
        {
            s_NodeHost.DeactivateAndUnload();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void WaitUntilStopRequested()
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine();
            System.Console.WriteLine("{0:HH:mm:ss.fff} UP AND RUNNING. PRESS CTRL + BREAK TO STOP", DateTime.UtcNow);
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Gray;

            s_StopRequested.WaitOne();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            s_StopRequested.Set();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static void GenerateDummyNodeHostConfig()
        {
            s_NodeHostConfig = new NodeHostConfig()
            {
                ApplicationName = "Test App",
                NodeName = "Test Node",
                ApplicationModules = new List<NodeHostConfig.ModuleConfig> {
                    new NodeHostConfig.ModuleConfig {
                        Assembly = "Dummy.Module"
                    }
                }
            };

            var serializer = new DataContractSerializer(typeof(NodeHostConfig));
            using ( var file = File.Create(PathUtility.LocalBinPath(NodeHostConfig.DefaultFileName)) )
            {
                serializer.WriteObject(file, s_NodeHostConfig);
                file.Flush();
            }

            s_NodeHostConfig = null;
        }

    }
}
