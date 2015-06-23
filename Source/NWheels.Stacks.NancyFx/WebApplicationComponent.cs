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
using NWheels.Hosting;
using NWheels.UI;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationComponent : LifecycleEventListenerBase
    {
        private readonly WebAppEndpointRegistration _endpointRegistration;
        private readonly IWebApplicationLogger _logger;
        private readonly UidlApplication _application;
        private readonly UidlDocument _uidl;
        private NancyHost _host;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationComponent(
            IComponentContext components, 
            WebAppEndpointRegistration endpointRegistration, 
            ITypeMetadataCache metadataCache,
            IUILocalizationProvider localizationProvider,
            IWebApplicationLogger logger)
        {
            _endpointRegistration = endpointRegistration;
            _logger = logger;
            _application = (UidlApplication)components.Resolve(endpointRegistration.Contract);
            _uidl = UidlBuilder.GetApplicationDocument(_application, metadataCache, localizationProvider);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            var module = new WebApplicationModule(_uidl);
            var bootstrapper = new WebApplicationBootstrapper(module);

            _logger.WebApplicationActivating(_application.IdName, _endpointRegistration.Address, module.ContentRootPath);

            _host = new NancyHost(bootstrapper, new[] { TrailingSlashSafeUri(_endpointRegistration.Address) });
            
            _logger.WebApplicationActive(_application.IdName, _endpointRegistration.Address);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            _host.Start();
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
