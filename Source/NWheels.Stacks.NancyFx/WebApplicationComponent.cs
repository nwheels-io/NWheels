using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy.Bootstrappers.Autofac;
using Nancy.Hosting.Self;
using NWheels.Endpoints;
using NWheels.Hosting;
using NWheels.UI;
using NWheels.UI.Core;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationComponent : LifecycleEventListenerBase
    {
        private readonly WebAppEndpointRegistration _endpointRegistration;
        private readonly IWebApplicationLogger _logger;
        private readonly ApplicationDescription _application;
        private NancyHost _host;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationComponent(IComponentContext components, WebAppEndpointRegistration endpointRegistration, IWebApplicationLogger logger)
        {
            _endpointRegistration = endpointRegistration;
            _logger = logger;
            _application = ((IDescriptionProvider<ApplicationDescription>)components.Resolve(endpointRegistration.Contract)).GetDescription();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            var module = new WebApplicationModule(_application);
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
