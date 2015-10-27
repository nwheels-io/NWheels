using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using Nancy.Bootstrappers.Autofac;
using Nancy.Hosting.Self;
using NWheels.Authorization;
using NWheels.DataObjects;
using NWheels.Endpoints;
using NWheels.Entities.Core;
using NWheels.Globalization;
using NWheels.Hosting;
using NWheels.UI;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationComponent : LifecycleEventListenerBase, IWebModuleContext
    {
        private readonly IComponentContext _components;
        private readonly WebAppEndpointRegistration _endpointRegistration;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ILocalizationProvider _localizationProvider;
        private readonly WebApiDispatcherFactory _dispatcherFactory;
        private readonly WebModuleLoggingHook _loggingHook;
        private readonly ISessionManager _sessionManager;
        private readonly IWebApplicationLogger _logger;
        private readonly IFrameworkUIConfig _frameworkUIConfig;
        private readonly UidlApplication _application;
        private readonly ApplicationEntityService _entityService;
        private readonly Dictionary<string, object> _apiServicesByContractName;
        private readonly Dictionary<string, WebApiDispatcherBase> _apiDispatchersByContractName;
        private UidlDocument _uidl;
        private WebApplicationBootstrapper _hostBootstrapper;
        private NancyHost _host;
        private WebApplicationModule _module;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationComponent(
            IComponentContext components, 
            WebAppEndpointRegistration endpointRegistration, 
            ITypeMetadataCache metadataCache,
            ILocalizationProvider localizationProvider,
            WebApiDispatcherFactory dispatcherFactory,
            WebModuleLoggingHook loggingHook,
            ISessionManager sessionManager,
            IWebApplicationLogger logger,
            IFrameworkUIConfig frameworkUIConfig)
        {
            _components = components;
            _endpointRegistration = endpointRegistration;
            _metadataCache = metadataCache;
            _localizationProvider = localizationProvider;
            _dispatcherFactory = dispatcherFactory;
            _sessionManager = sessionManager;
            _loggingHook = loggingHook;
            _logger = logger;
            _frameworkUIConfig = frameworkUIConfig;

            _application = (UidlApplication)components.Resolve(endpointRegistration.Contract);
            _entityService = new ApplicationEntityService(
                components.Resolve<IFramework>(), 
                components.Resolve<ITypeMetadataCache>(),
                components.Resolve<IDomainContextLogger>(), 
                _application.RequiredDomainContexts);
            
            _apiServicesByContractName = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            _apiDispatchersByContractName = new Dictionary<string, WebApiDispatcherBase>(StringComparer.InvariantCultureIgnoreCase);
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

        public ApplicationEntityService EntityService
        {
            get { return _entityService; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SkinName 
        {
            get { return _application.DefaultSkin; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SkinSubFolderName
        {
            get { return "skin." + this.SkinName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string BaseSubFolderName
        {
            get { return "base"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IRootPathProvider IWebModuleContext.PathProvider 
        {
            get { return _hostBootstrapper; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IWebApplicationLogger IWebModuleContext.Logger
        {
            get { return _logger; }
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
            _uidl = UidlBuilder.GetApplicationDocument(_application, _metadataCache, _localizationProvider, _components);
            BuildApiDispatchers(_components, _dispatcherFactory);

            _module = _components.Resolve<WebApplicationModule>(TypedParameter.From<IWebModuleContext>(this));
            var sessionHook = new WebModuleSessionHook(_module, _sessionManager, _logger);

            _hostBootstrapper = new WebApplicationBootstrapper(this, _module, _loggingHook, sessionHook, _frameworkUIConfig);
            _host = new NancyHost(_hostBootstrapper, new[] { TrailingSlashSafeUri(_endpointRegistration.Address) });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            _logger.WebApplicationActivating(_application.IdName, _endpointRegistration.Address, _hostBootstrapper.GetRootPath());
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
