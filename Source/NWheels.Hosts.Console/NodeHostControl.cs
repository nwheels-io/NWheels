using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Autofac;
using Autofac.Core;
using NWheels.Configuration.Core;
using NWheels.Configuration.Impls;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging.Core;
using NWheels.Stacks.Nlog;
using NWheels.Utilities;
using Topshelf;
using Topshelf.Runtime;
using Topshelf.ServiceConfigurators;

namespace NWheels.Hosts.Console
{
    public class NodeHostControl : ServiceControl
    {
        private readonly ProgramConfig _hostConfig;
        private NodeHost _nodeHost;
        private HostControl _hostControl;
        private bool _startedSuccessfully;
        private IPlainLog _log;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeHostControl(ProgramConfig hostConfig)
        {
            _hostConfig = hostConfig;
            _hostConfig.BootConfig.InstanceId = hostConfig.HostSettings.InstanceName;

            NLogBasedPlainLog.Instance.ConsoleLogLevel = NLogBasedPlainLog.ToNLogLevel(hostConfig.LogLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ServiceControl

        public bool Start(HostControl hostControl)
        {
            _hostControl = hostControl;

            bool isInConsoleMode = hostControl.IsRunningAsConsole();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            _log = NLogBasedPlainLog.Instance;
            if ( isInConsoleMode )
            {
                _log.ConfigureConsoleOutput();
            }
            
            _log.Info("NWheels Console Host version {0}", typeof(Program).Assembly.GetName().Version);

            //_s_isBatchJobMode = Environment.GetCommandLineArgs().Any(IsBatchJobModeArgument);

            if ( _hostConfig.IsBatchJob )
            {
                _log.Info("Running in batch job mode.");
            }

            try
            {
                StartNodeHost();

                if ( isInConsoleMode && !_hostConfig.IsBatchJob )
                {
                    AnnounceRunningOnConsole();
                }
            }
            catch ( Exception e )
            {
                _log.Critical("NODE FAILED TO START! {0}", e.ToString());
                hostControl.Stop();
                Environment.Exit(-100);
            }

            if ( _hostConfig.IsBatchJob )
            {
                _log.Info("DONE - IN BATCH JOB MODE");
                hostControl.Stop();
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Stop(HostControl hostControl)
        {
            try
            {
                _nodeHost.DeactivateAndUnload();
            }
            catch ( Exception e )
            {
                _log.Warning("NODE WAS NOT CORRECTLY STOPPED! {0}", e.ToString());
            }
            return true;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var assemblyProbePaths = new List<string>();

            assemblyProbePaths.Add(Path.Combine(_hostConfig.BootConfig.LoadedFromDirectory, assemblyName.Name + ".dll"));
            assemblyProbePaths.Add(PathUtility.HostBinPath(assemblyName.Name + ".dll"));

            foreach ( var probePath in assemblyProbePaths )
            {
                if ( File.Exists(probePath) )
                {
                    return Assembly.LoadFrom(probePath);
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StartNodeHost()
        {
            _nodeHost = new NodeHost(_hostConfig.BootConfig, RegisterHostComponents);
            _nodeHost.StateChanged += OnNodeHostStateChanged;
            _nodeHost.LoadAndActivate();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnNodeHostStateChanged(object sender, EventArgs e)
        {
            if (_nodeHost.State == NodeState.Active)
            {
                _startedSuccessfully = true;
            }
            else if (_startedSuccessfully && _nodeHost.State == NodeState.Down)
            {
                _log.Info("DONE - STOPPED");
                _hostControl.Stop();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterHostComponents(ContainerBuilder builder)
        {
            builder.RegisterModule<NWheels.Stacks.Nlog.ModuleLoader>();
            builder.RegisterType<CommandLineConfigurationSource>()
                .WithParameter(TypedParameter.From(_hostConfig.CommandLineConfigValues.ToArray()))
                .As<IConfigurationSource>()
                .LastInPipeline();
            
            //builder.RegisterInstance(new CommandLineConfigurationLoader.CommandLineParameters(_hostConfig.CommandLineConfigValues.ToArray()));
            //builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<CommandLineConfigurationLoader>().AnchoredFirstInPipeline();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void AnnounceRunningOnConsole()
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine();
            System.Console.WriteLine("{0:HH:mm:ss.fff} UP AND RUNNING. PRESS CTRL + BREAK TO STOP", DateTime.UtcNow);
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
