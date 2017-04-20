using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public class HttpEndpointInjectorPort : InjectorPort
    {
        public HttpEndpointInjectorPort(IHttpEndpointConfiguration configuration, Func<HttpContext, Task> handler)
        {
            this.Configuration = configuration;
            this.Handler = handler;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IHttpEndpointConfiguration Configuration { get; }
        public Func<HttpContext, Task> Handler { get; }
    }
}
