using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Gemini.Modules.Output;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging.Core;
using NWheels.Processing.Workflows;
using NWheels.Testing.Controllers;
using NWheels.Tools.TestBoard.Messages;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;
using NWheels.Utilities;

namespace NWheels.Tools.TestBoard.Services
{
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [Export(typeof(IApplicationControllerService))]
    public class ApplicationControllerService : IApplicationControllerService
    {
        private readonly object _syncRoot = new object();
        private readonly IPlainLog _plainLog;
        private readonly IOutput _output;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IApplicationComponentInjector[] _componentInjectors;
        private readonly List<ApplicationController> _applications;
        private readonly Dictionary<string, ApplicationController> _applicationByFilePath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ApplicationControllerService(
            IPlainLog plainLog,
            IOutput output,
            IMessageBoxService messageBoxService,
            IEventAggregator eventAggregator, 
            [ImportMany] IEnumerable<IApplicationComponentInjector> componentInjectors)
        {
            _output = output;
            _plainLog = plainLog;
            _messageBoxService = messageBoxService;
            _eventAggregator = eventAggregator;
            _componentInjectors = componentInjectors.ToArray();

            _applications = new List<ApplicationController>();
            _applicationByFilePath = new Dictionary<string, ApplicationController>(StringComparer.InvariantCultureIgnoreCase);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Open(string bootConfigFilePath, bool autoRun = false)
        {
            _output.AppendLine(string.Format("Loading application from file: {0}.", bootConfigFilePath));

            var bootConfig = BootConfiguration.LoadFromFile(bootConfigFilePath);
            bootConfig.Validate();

            using ( TakeExclusiveAccess() )
            {
                if ( !_applicationByFilePath.ContainsKey(bootConfigFilePath) )
                {
                    var application = new ApplicationController(_plainLog, bootConfig);
                    application.InjectingComponents += OnInjectingHostComponents;

                    _applications.Add(application);
                    _applicationByFilePath.Add(bootConfigFilePath, application);

                    application.CurrentStateChanged += OnApplicationControllerStateChanged;
                    _eventAggregator.Publish(new AppOpenedMessage(application, autoRun), action => Task.Run(action));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Close(ApplicationController application)
        {
            _output.AppendLine(string.Format("Closing application: {0}.", application.DisplayName));

            using ( TakeExclusiveAccess() )
            {
                if ( _applicationByFilePath.ContainsValue(application) )
                {
                    _applicationByFilePath.Remove(_applicationByFilePath.First(kvp => kvp.Value == application).Key);
                    _applications.Remove(application);

                    _eventAggregator.Publish(new AppClosedMessage(application), action => Task.Run(action));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CloseAll()
        {
            _output.AppendLine(string.Format("Closing all applications."));

            var applicationsSnapshot = _applications.ToArray();

            foreach ( var application in applicationsSnapshot )
            {
                Close(application);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<ApplicationController> Applications
        {
            get { return _applications; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var assemblyProbePaths = new List<string>();

            assemblyProbePaths.Add(PathUtility.HostBinPath("..\\Core\\" + assemblyName.Name + ".dll"));
            assemblyProbePaths.Add(PathUtility.HostBinPath("..\\Core\\" + assemblyName.Name + ".exe"));

            foreach (var bootConfigLoadPath in _applicationByFilePath.Keys)
            {
                assemblyProbePaths.Add(Path.Combine(Path.GetDirectoryName(bootConfigLoadPath), assemblyName.Name + ".dll"));
                assemblyProbePaths.Add(Path.Combine(Path.GetDirectoryName(bootConfigLoadPath), assemblyName.Name + ".exe"));
            }

            foreach (var probePath in assemblyProbePaths)
            {
                if (File.Exists(probePath))
                {
                    return Assembly.LoadFrom(probePath);
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ExclusiveAccess TakeExclusiveAccess()
        {
            return new ExclusiveAccess(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnInjectingHostComponents(object sender, ComponentInjectionEventArgs e)
        {
            e.ContainerBuilder.RegisterInstance(_plainLog).As<IPlainLog>();
            e.ContainerBuilder.RegisterInstance(new AssemblySearchPathProvider()).As<IAssemblySearchPathProvider>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnApplicationControllerStateChanged(object sender, ControllerStateEventArgs e)
        {
            _eventAggregator.Publish(new AppStateChangedMessage((ApplicationController)e.Controller), action => action());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AssemblySearchPathProvider : DefaultAssemblySearchPathProvider
        {
            public override string[] GetAssemblySearchPaths(BootConfiguration node, BootConfiguration.ModuleConfig module)
            {
                var baseSearchPaths = base.GetAssemblySearchPaths(node, module);
                var devCoreBinFolder = PathUtility.HostBinPath("..\\Core");

                if ( Directory.Exists(devCoreBinFolder) )
                {
                    var searchPaths = baseSearchPaths.Concat(new[] {
                        Path.Combine(devCoreBinFolder, module.Assembly)
                    }).ToArray();
                    
                    return searchPaths;
                }
                else
                {
                    return baseSearchPaths;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ExclusiveAccess : IDisposable
        {
            private readonly ApplicationControllerService _owner;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ExclusiveAccess(ApplicationControllerService owner)
            {
                _owner = owner;

                if ( !Monitor.TryEnter(_owner._syncRoot, 10000) )
                {
                    throw new TimeoutException("An internal concurrency error encountered");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                Monitor.Exit(_owner._syncRoot);
            }
        }
    }
}
