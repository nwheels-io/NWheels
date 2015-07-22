using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging.Core;
using NWheels.Processing.Workflows;
using NWheels.Testing.Controllers;
using NWheels.Tools.TestBoard.Messages;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;

namespace NWheels.Tools.TestBoard.Services
{
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [Export(typeof(IApplicationControllerService))]
    public class ApplicationControllerService : IApplicationControllerService
    {
        private readonly object _syncRoot = new object();
        private readonly IPlainLog _plainLog;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IApplicationComponentInjector[] _componentInjectors;
        private readonly List<ApplicationController> _applications;
        private readonly Dictionary<string, ApplicationController> _applicationByFilePath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ApplicationControllerService(
            IPlainLog plainLog,
            IMessageBoxService messageBoxService,
            IEventAggregator eventAggregator, 
            [ImportMany] IEnumerable<IApplicationComponentInjector> componentInjectors)
        {
            _plainLog = plainLog;
            _messageBoxService = messageBoxService;
            _eventAggregator = eventAggregator;
            _componentInjectors = componentInjectors.ToArray();

            _applications = new List<ApplicationController>();
            _applicationByFilePath = new Dictionary<string, ApplicationController>(StringComparer.InvariantCultureIgnoreCase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Open(string bootConfigFilePath)
        {
            var bootConfig = BootConfiguration.LoadFromFile(bootConfigFilePath);

            using ( TakeExclusiveAccess() )
            {
                if ( !_applicationByFilePath.ContainsKey(bootConfigFilePath) )
                {
                    var newController = new ApplicationController(_plainLog, bootConfig, InjectHostComponents);
                    _applications.Add(newController);
                    _applicationByFilePath.Add(bootConfigFilePath, newController);

                    _eventAggregator.Publish(new AppOpenedMessage(newController), action => Task.Run(action));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Close(ApplicationController application)
        {
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

        private ExclusiveAccess TakeExclusiveAccess()
        {
            return new ExclusiveAccess(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InjectHostComponents(Autofac.ContainerBuilder containerBuilder)
        {
            //containerBuilder
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
