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

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationComponent : LifecycleEventListenerBase
    {
        private readonly IComponentContext _components;
        private readonly WebAppEndpointRegistration _endpointRegistration;
        private NancyHost _host;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationComponent(IComponentContext components, WebAppEndpointRegistration endpointRegistration)
        {
            _endpointRegistration = endpointRegistration;
            _components = components;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            //var bootstrapper = new WebApplicationBootstrapper(_components, );
            _host = new NancyHost();
        }
    }
}
