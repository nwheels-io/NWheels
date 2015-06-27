using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy.Bootstrappers.Autofac;
using Nancy.Hosting.Self;
using NWheels.DataObjects;
using NWheels.Endpoints;
using NWheels.Globalization;
using NWheels.Hosting;
using NWheels.UI;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationComponent : LifecycleEventListenerBase, IWebModuleContext
    {
        private readonly WebAppEndpointRegistration _endpointRegistration;
        private readonly IWebApplicationLogger _logger;
        private readonly UidlApplication _application;
        private readonly UidlDocument _uidl;
        private readonly Dictionary<string, object> _apiServicesByContractName;
        private readonly Dictionary<string, WebApiDispatcherBase> _apiDispatchersByContractName;
        private NancyHost _host;
        private WebApplicationModule _module;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationComponent(
            IComponentContext components, 
            WebAppEndpointRegistration endpointRegistration, 
            ITypeMetadataCache metadataCache,
            ILocalizationProvider localizationProvider,
            WebApiDispatcherFactory dispatcherFactory,
            IWebApplicationLogger logger)
        {
            _endpointRegistration = endpointRegistration;
            _logger = logger;

            _application = (UidlApplication)components.Resolve(endpointRegistration.Contract);
            _uidl = UidlBuilder.GetApplicationDocument(_application, metadataCache, localizationProvider);
            
            _apiServicesByContractName = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            _apiDispatchersByContractName = new Dictionary<string, WebApiDispatcherBase>(StringComparer.InvariantCultureIgnoreCase);

            BuildApiDispatchers(components, dispatcherFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildApiDispatchers(IComponentContext components, WebApiDispatcherFactory dispatcherFactory)
        {
            foreach ( var apiContractType in _application.RequiredDomainApis )
            {
                var contractName = apiContractType.Name;
                var service = components.Resolve(apiContractType);
                var dispatcher = dispatcherFactory.CreateDispatcher(apiContractType);

                _apiServicesByContractName.Add(contractName, service);
                _apiDispatchersByContractName.Add(contractName, dispatcher);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IWebModuleContext Members

        UidlDocument IWebModuleContext.Uidl
        {
            get { return _uidl; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        UidlApplication IWebModuleContext.Application
        {
            get { return _application; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyDictionary<string, object> IWebModuleContext.ApiServicesByContractName
        {
            get { return _apiServicesByContractName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyDictionary<string, WebApiDispatcherBase> IWebModuleContext.ApiDispatchersByContractName
        {
            get { return _apiDispatchersByContractName; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            _module = new WebApplicationModule(this);
            var bootstrapper = new WebApplicationBootstrapper(_module);

            _host = new NancyHost(bootstrapper, new[] { TrailingSlashSafeUri(_endpointRegistration.Address) });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            _logger.WebApplicationActivating(_application.IdName, _endpointRegistration.Address, _module.ContentRootPath);
            _host.Start();
            _logger.WebApplicationActive(_application.IdName, _endpointRegistration.Address);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            _host.Stop();
            _logger.WebApplicationDeactivated(_application.IdName, _endpointRegistration.Address);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Unload()
        {
            _host.Dispose();
            _host = null;
            _module = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Uri TrailingSlashSafeUri(Uri uri)
        {
            var uriString = uri.ToString();

            if ( uriString.EndsWith("/") )
            {
                return uri;
            }

            return new Uri(uriString + "/");
        }
    }
}
