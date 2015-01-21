using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NWheels.Core.Hosting;
using NWheels.Core.Logging;
using NWheels.Hosting;
using NWheels.Utilities;

namespace NWheels.Hosts.Console
{
    class Program
    {
        private static NodeHostConfig s_NodeHostConfig;
        private static NodeHost s_NodeHost;
        private static ManualResetEvent s_StopRequested;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static int Main(string[] args)
        {
            s_NodeHostConfig = new NodeHostConfig() {
                ApplicationName = "Test App",
                NodeName = "Test Node",
                FrameworkModules = new[] { "FX Module 1", "FX Module 2" },
                ApplicationModules = new[] { "App Module 1", "App Module 2" }
            };

            var serializer = new DataContractSerializer(typeof(NodeHostConfig));
            using ( var file = File.Create(PathUtility.LocalBinPath(NodeHostConfig.DefaultFileName)) )
            {
                serializer.WriteObject(file, s_NodeHostConfig);
                file.Flush();
            }

            s_NodeHostConfig = null;

            CrashLog.RegisterUnhandledExceptionHandler();
            PlainLog.ConfigureConsoleOutput();

            PlainLog.Info("NWheels Console Host version {0}", typeof(Program).Assembly.GetName().Version);

            try
            {
                LoadNodeHostConfig();
            }
            catch ( Exception e )
            {
                PlainLog.Critical("FAILED TO LOAD {0}: {1}", NodeHostConfig.DefaultFileName, e.Message);
                return 1;
            }

            try
            {
                StartNodeHost();
            }
            catch ( Exception e )
            {
                PlainLog.Critical("NODE FAILED TO START! {0}", e.ToString());
                return 2;
            }

            WaitUntilStopRequested();

            try
            {
                StopNodeHost();
            }
            catch ( Exception e )
            {
                PlainLog.Warning("NODE WAS NOT CORRECTLY STOPPED! {0}", e.ToString());
            }

            return 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void LoadNodeHostConfig()
        {
            PlainLog.Debug("Loading {0}", NodeHostConfig.DefaultFileName);

            s_NodeHostConfig = NodeHostConfig.LoadFromFile(PathUtility.LocalBinPath(NodeHostConfig.DefaultFileName));
            s_NodeHostConfig.Validate();

            PlainLog.Debug("> Application Name   - {0}", s_NodeHostConfig.ApplicationName);
            PlainLog.Debug("> Node Name          - {0}", s_NodeHostConfig.NodeName);

            foreach ( var moduleString in s_NodeHostConfig.FrameworkModules )
            {
                PlainLog.Debug("> Framework Module   - {0}", moduleString);
            }

            foreach ( var moduleString in s_NodeHostConfig.ApplicationModules )
            {
                PlainLog.Debug("> Application Module - {0}", moduleString);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static void StartNodeHost()
        {
            PlainLog.Info("Starting node...");

            s_NodeHost = new NodeHost(s_NodeHostConfig);
            s_NodeHost.LoadAndActivate();

            s_StopRequested = new ManualResetEvent(initialState: false);
            System.Console.CancelKeyPress += Console_CancelKeyPress;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void StopNodeHost()
        {
            PlainLog.Info("Stopping node...");
            s_NodeHost.DeactivateAndUnload();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void WaitUntilStopRequested()
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine();
            System.Console.WriteLine("------ UP AND RUNNING. PRESS CTRL + BREAK TO STOP ------");
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
    }
}
