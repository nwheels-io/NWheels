using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Authorization;
using NWheels.DataObjects;
using NWheels.Endpoints;
using NWheels.Entities.Core;
using NWheels.Globalization;
using NWheels.Hosting;
using NWheels.UI;
using NWheels.UI.Core;
using NWheels.UI.Factories;
using NWheels.UI.Uidl;
using NWheels.Utilities;

namespace NWheels.Stacks.AspNet
{
    public class UidlApplicationComponent : LifecycleEventListenerBase, IWebModuleContext
    {
        private readonly IComponentContext _components;
        private readonly WebAppEndpointRegistration _endpointRegistration;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ILocalizationProvider _localizationProvider;
        private readonly ISessionManager _sessionManager;
        private readonly IWebApplicationLogger _logger;
        private readonly IFrameworkUIConfig _frameworkUIConfig;
        private readonly UidlApplication _application;
        private readonly ApplicationEntityService _entityService;
        private readonly Dictionary<string, object> _apiServicesByContractName;
        private readonly string _contentRootPath;
        private UidlDocument _uidl;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlApplicationComponent(
            IComponentContext components,
            WebAppEndpointRegistration endpointRegistration,
            ITypeMetadataCache metadataCache,
            ILocalizationProvider localizationProvider,
            ISessionManager sessionManager,
            IWebApplicationLogger logger,
            IFrameworkUIConfig frameworkUIConfig)
        {
            _components = components;
            _endpointRegistration = endpointRegistration;
            _metadataCache = metadataCache;
            _localizationProvider = localizationProvider;
            _sessionManager = sessionManager;
            _logger = logger;
            _frameworkUIConfig = frameworkUIConfig;
            _contentRootPath = PathUtility.GetAbsolutePath(frameworkUIConfig.WebContentRootPath, relativeTo: PathUtility.HostBinPath());

            _application = (UidlApplication)components.Resolve(endpointRegistration.Contract);
            _entityService = new ApplicationEntityService(
                components.Resolve<IFramework>(),
                components.Resolve<ITypeMetadataCache>(),
                components.Resolve<IViewModelObjectFactory>(),
                components.Resolve<IEnumerable<IJsonSerializationExtension>>(),
                components.Resolve<IDomainContextLogger>(),
                _application.RequiredDomainContexts);

            _apiServicesByContractName = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
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

        string IWebModuleContext.ContentRootPath
        {
            get { return _contentRootPath; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IWebApplicationLogger IWebModuleContext.Logger
        {
            get { return _logger; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            _uidl = UidlBuilder.GetApplicationDocument(_application, _metadataCache, _localizationProvider, _components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Unload()
        {
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
